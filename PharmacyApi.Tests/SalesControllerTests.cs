using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PharmacyApi.Controllers;
using PharmacyApi.Models;
using PharmacyApi.Services;

namespace PharmacyApi.Tests;

public class SalesControllerTests
{
    private readonly Mock<ISaleService> _mockService;
    private readonly SalesController _controller;

    public SalesControllerTests()
    {
        _mockService = new Mock<ISaleService>();
        _controller = new SalesController(_mockService.Object);
    }

    // -----------------------------------------------------------------------
    // GetAll
    // -----------------------------------------------------------------------

    [Fact]
    public void GetAll_SalesExist_ReturnsOkWithAllSales()
    {
        // Arrange
        var sales = new List<Sale>
        {
            new() { Id = 1, MedicineId = 1, MedicineName = "Paracetamol 500mg",
                    QuantitySold = 5, UnitPrice = 25.50m, TotalPrice = 127.50m,
                    SaleDate = DateTime.UtcNow },
            new() { Id = 2, MedicineId = 2, MedicineName = "Ibuprofen 400mg",
                    QuantitySold = 2, UnitPrice = 42.00m, TotalPrice = 84.00m,
                    SaleDate = DateTime.UtcNow }
        };
        _mockService.Setup(s => s.GetAll()).Returns(sales);

        // Act
        var result = _controller.GetAll();

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);

        var returned = ok.Value as IEnumerable<Sale>;
        returned.Should().HaveCount(2);
    }

    [Fact]
    public void GetAll_NoSales_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(new List<Sale>());

        // Act
        var result = _controller.GetAll();

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);

        var returned = ok.Value as IEnumerable<Sale>;
        returned.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // Create — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_ValidRequest_Returns201WithSale()
    {
        // Arrange
        var request = new CreateSaleRequest { MedicineId = 1, QuantitySold = 3 };

        var sale = new Sale
        {
            Id = 1,
            MedicineId = 1,
            MedicineName = "Paracetamol 500mg",
            QuantitySold = 3,
            UnitPrice = 25.50m,
            TotalPrice = 76.50m,
            SaleDate = DateTime.UtcNow
        };
        _mockService.Setup(s => s.RecordSale(request)).Returns(sale);

        // Act
        var result = _controller.Create(request);

        // Assert
        var created = result.Result as CreatedAtActionResult;
        created.Should().NotBeNull();
        created!.StatusCode.Should().Be(201);

        var returned = created.Value as Sale;
        returned!.Id.Should().Be(1);
        returned.TotalPrice.Should().Be(76.50m);
    }

    [Fact]
    public void Create_ValidRequest_CallsRecordSaleOnce()
    {
        // Arrange
        var request = new CreateSaleRequest { MedicineId = 1, QuantitySold = 2 };
        var sale = new Sale
        {
            Id = 1,
            MedicineId = 1,
            MedicineName = "Paracetamol 500mg",
            QuantitySold = 2,
            UnitPrice = 25.50m,
            TotalPrice = 51.00m,
            SaleDate = DateTime.UtcNow
        };
        _mockService.Setup(s => s.RecordSale(request)).Returns(sale);

        // Act
        _controller.Create(request);

        // Assert — verify the service was actually called
        _mockService.Verify(s => s.RecordSale(request), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Create — failure paths
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_InsufficientStock_Returns400WithErrorMessage()
    {
        // Arrange
        var request = new CreateSaleRequest { MedicineId = 1, QuantitySold = 999 };
        _mockService
            .Setup(s => s.RecordSale(request))
            .Throws(new InvalidOperationException(
                "Insufficient stock for 'Paracetamol 500mg'. Available: 50, requested: 999."));

        // Act
        var result = _controller.Create(request);

        // Assert
        var bad = result.Result as BadRequestObjectResult;
        bad.Should().NotBeNull();
        bad!.StatusCode.Should().Be(400);

        // Verify the error message is passed through to the client
        var body = bad.Value;
        body!.ToString().Should().Contain("Insufficient stock");
    }

    [Fact]
    public void Create_MedicineNotFound_Returns400WithErrorMessage()
    {
        // Arrange
        var request = new CreateSaleRequest { MedicineId = 999, QuantitySold = 1 };
        _mockService
            .Setup(s => s.RecordSale(request))
            .Throws(new InvalidOperationException("Medicine with id 999 was not found."));

        // Act
        var result = _controller.Create(request);

        // Assert
        var bad = result.Result as BadRequestObjectResult;
        bad.Should().NotBeNull();
        bad!.StatusCode.Should().Be(400);
        bad.Value!.ToString().Should().Contain("999");
    }
}