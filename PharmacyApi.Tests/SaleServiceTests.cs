using FluentAssertions;
using Moq;
using PharmacyApi.Models;
using PharmacyApi.Services;

namespace PharmacyApi.Tests;

public class SaleServiceTests
{
    private readonly Mock<IMedicineService> _mockMedicineService;

    public SaleServiceTests()
    {
        _mockMedicineService = new Mock<IMedicineService>();
    }

    // Helper — creates a SaleService with a real in-memory temp store
    private SaleService CreateService()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        var store = new JsonFileStore<Sale>(tempFile);
        return new SaleService(store, _mockMedicineService.Object);
    }

    // -----------------------------------------------------------------------
    // GetAll
    // -----------------------------------------------------------------------

    [Fact]
    public void GetAll_NoSales_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.GetAll();

        // Assert
        result.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // RecordSale — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void RecordSale_ValidRequest_ReturnsSaleWithCorrectTotal()
    {
        // Arrange
        var medicine = new Medicine
        {
            Id = 1,
            FullName = "Paracetamol 500mg",
            Brand = "Crocin",
            ExpiryDate = DateTime.Now.AddYears(1),
            Quantity = 50,
            Price = 25.50m
        };

        // TryReduceStock succeeds — sets the out parameter and returns true
        _mockMedicineService
            .Setup(s => s.TryReduceStock(1, 5, out medicine, out It.Ref<string?>.IsAny))
            .Returns(true);

        var service = CreateService();
        var request = new CreateSaleRequest { MedicineId = 1, QuantitySold = 5 };

        // Act
        var sale = service.RecordSale(request);

        // Assert
        sale.MedicineId.Should().Be(1);
        sale.MedicineName.Should().Be("Paracetamol 500mg");
        sale.QuantitySold.Should().Be(5);
        sale.UnitPrice.Should().Be(25.50m);
        sale.TotalPrice.Should().Be(127.50m);   // 25.50 × 5
        sale.SaleDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RecordSale_ValidRequest_SaleIsPersistedAndReturnedByGetAll()
    {
        // Arrange
        var medicine = new Medicine
        {
            Id = 1,
            FullName = "Ibuprofen 400mg",
            Brand = "Brufen",
            ExpiryDate = DateTime.Now.AddYears(1),
            Quantity = 30,
            Price = 42.00m
        };

        _mockMedicineService
            .Setup(s => s.TryReduceStock(1, 2, out medicine, out It.Ref<string?>.IsAny))
            .Returns(true);

        var service = CreateService();

        // Act
        service.RecordSale(new CreateSaleRequest { MedicineId = 1, QuantitySold = 2 });
        var allSales = service.GetAll().ToList();

        // Assert
        allSales.Should().HaveCount(1);
        allSales[0].TotalPrice.Should().Be(84.00m);   // 42.00 × 2
    }

    // -----------------------------------------------------------------------
    // RecordSale — failure paths
    // -----------------------------------------------------------------------

    [Fact]
    public void RecordSale_MedicineNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        Medicine? nullMedicine = null;
        var errorMessage = "Medicine with id 999 was not found.";

        _mockMedicineService
            .Setup(s => s.TryReduceStock(999, 1, out nullMedicine, out errorMessage))
            .Returns(false);

        var service = CreateService();
        var request = new CreateSaleRequest { MedicineId = 999, QuantitySold = 1 };

        // Act
        Action act = () => service.RecordSale(request);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Medicine with id 999 was not found.");
    }

    [Fact]
    public void RecordSale_InsufficientStock_ThrowsInvalidOperationException()
    {
        // Arrange
        Medicine? medicine = new Medicine
        {
            Id = 1,
            FullName = "Vitamin C 500mg",
            Brand = "Limcee",
            Quantity = 3,
            Price = 150.00m,
            ExpiryDate = DateTime.Now.AddYears(1)
        };
        var errorMessage = "Insufficient stock for 'Vitamin C 500mg'. Available: 3, requested: 10.";

        _mockMedicineService
            .Setup(s => s.TryReduceStock(1, 10, out medicine, out errorMessage))
            .Returns(false);

        var service = CreateService();
        var request = new CreateSaleRequest { MedicineId = 1, QuantitySold = 10 };

        // Act
        Action act = () => service.RecordSale(request);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Insufficient stock for 'Vitamin C 500mg'. Available: 3, requested: 10.");
    }

    [Fact]
    public void RecordSale_MultipleSales_IdsAreSequential()
    {
        // Arrange
        var medicine = new Medicine
        {
            Id = 1,
            FullName = "Cetirizine 10mg",
            Brand = "Cetzine",
            Quantity = 100,
            Price = 18.75m,
            ExpiryDate = DateTime.Now.AddYears(1)
        };

        _mockMedicineService
            .Setup(s => s.TryReduceStock(1, It.IsAny<int>(), out medicine, out It.Ref<string?>.IsAny))
            .Returns(true);

        var service = CreateService();

        // Act
        var sale1 = service.RecordSale(new CreateSaleRequest { MedicineId = 1, QuantitySold = 1 });
        var sale2 = service.RecordSale(new CreateSaleRequest { MedicineId = 1, QuantitySold = 2 });
        var sale3 = service.RecordSale(new CreateSaleRequest { MedicineId = 1, QuantitySold = 3 });

        // Assert
        sale1.Id.Should().Be(1);
        sale2.Id.Should().Be(2);
        sale3.Id.Should().Be(3);
    }
}