using FluentAssertions;
using PharmacyApi.Models;
using PharmacyApi.Services;

namespace PharmacyApi.Tests;

// MedicineService has no external interface dependency — it only depends on
// JsonFileStore<Medicine>, which we give a real temp file for each test.
// This means these are slightly closer to integration tests, but they run
// in milliseconds because there's no network or database involved.
public class MedicineServiceTests
{
    // Creates a fresh service backed by an isolated temp file for each test.
    // This guarantees tests never share state or interfere with each other.
    private static MedicineService CreateService()
    {
        // GetTempFileName() creates an empty file — JsonFileStore chokes on empty content.
        // Instead, build a unique path in the temp folder WITHOUT creating the file,
        // so JsonFileStore's constructor initialises it correctly with [].
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        var store = new JsonFileStore<Medicine>(tempFile);
        return new MedicineService(store);
    }

    private static CreateMedicineRequest SampleRequest(
        string name = "Paracetamol 500mg",
        string brand = "Crocin",
        int quantity = 50,
        decimal price = 25.50m) => new()
        {
            FullName = name,
            Brand = brand,
            ExpiryDate = DateTime.Now.AddYears(1),
            Quantity = quantity,
            Price = price,
            Notes = "Test notes"
        };

    // -----------------------------------------------------------------------
    // GetAll
    // -----------------------------------------------------------------------

    [Fact]
    public void GetAll_EmptyStore_ReturnsEmptyList()
    {
        var service = CreateService();

        service.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void GetAll_AfterAddingMedicines_ReturnsAll()
    {
        // Arrange
        var service = CreateService();
        service.Add(SampleRequest("Paracetamol 500mg"));
        service.Add(SampleRequest("Ibuprofen 400mg", "Brufen"));

        // Act
        var result = service.GetAll().ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    // -----------------------------------------------------------------------
    // GetById
    // -----------------------------------------------------------------------

    [Fact]
    public void GetById_ExistingId_ReturnsMedicine()
    {
        // Arrange
        var service = CreateService();
        var added = service.Add(SampleRequest());

        // Act
        var result = service.GetById(added.Id);

        // Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Paracetamol 500mg");
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var service = CreateService();

        service.GetById(999).Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // Add
    // -----------------------------------------------------------------------

    [Fact]
    public void Add_ValidRequest_AssignsId()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.Add(SampleRequest());

        // Assert
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Add_MultipleMedicines_IdsAreSequential()
    {
        // Arrange
        var service = CreateService();

        // Act
        var first = service.Add(SampleRequest("Paracetamol 500mg"));
        var second = service.Add(SampleRequest("Ibuprofen 400mg", "Brufen"));
        var third = service.Add(SampleRequest("Cetirizine 10mg", "Cetzine"));

        // Assert
        first.Id.Should().Be(1);
        second.Id.Should().Be(2);
        third.Id.Should().Be(3);
    }

    [Fact]
    public void Add_ValidRequest_MapsAllFieldsCorrectly()
    {
        // Arrange
        var service = CreateService();
        var request = SampleRequest("Metformin 500mg", "Glucophage", 60, 35.50m);

        // Act
        var result = service.Add(request);

        // Assert
        result.FullName.Should().Be("Metformin 500mg");
        result.Brand.Should().Be("Glucophage");
        result.Quantity.Should().Be(60);
        result.Price.Should().Be(35.50m);
        result.Notes.Should().Be("Test notes");
    }

    [Fact]
    public void Add_Medicine_IsPersisted_AndReturnedByGetAll()
    {
        // Arrange
        var service = CreateService();

        // Act
        service.Add(SampleRequest("Vitamin C 500mg", "Limcee", 5, 150.00m));
        var all = service.GetAll().ToList();

        // Assert — confirm it was actually written and can be read back
        all.Should().HaveCount(1);
        all[0].FullName.Should().Be("Vitamin C 500mg");
        all[0].Price.Should().Be(150.00m);
    }

    // -----------------------------------------------------------------------
    // Search
    // -----------------------------------------------------------------------

    [Fact]
    public void Search_MatchingName_ReturnsFilteredResults()
    {
        // Arrange
        var service = CreateService();
        service.Add(SampleRequest("Paracetamol 500mg"));
        service.Add(SampleRequest("Ibuprofen 400mg", "Brufen"));
        service.Add(SampleRequest("Paracetamol 1000mg", "Calpol"));

        // Act
        var result = service.Search("para").ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => m.FullName.Contains("Paracetamol",
                                         StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Search_IsCaseInsensitive()
    {
        // Arrange
        var service = CreateService();
        service.Add(SampleRequest("Paracetamol 500mg"));

        // Act — search in all cases
        var upper = service.Search("PARA").ToList();
        var lower = service.Search("para").ToList();
        var mixed = service.Search("Para").ToList();

        // Assert
        upper.Should().HaveCount(1);
        lower.Should().HaveCount(1);
        mixed.Should().HaveCount(1);
    }

    [Fact]
    public void Search_NoMatch_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateService();
        service.Add(SampleRequest("Paracetamol 500mg"));

        // Act
        var result = service.Search("xyz").ToList();

        // Assert
        result.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // TryReduceStock
    // -----------------------------------------------------------------------

    [Fact]
    public void TryReduceStock_SufficientStock_ReturnsTrueAndReducesQuantity()
    {
        // Arrange
        var service = CreateService();
        var medicine = service.Add(SampleRequest(quantity: 50));

        // Act
        var success = service.TryReduceStock(medicine.Id, 10, out var updated, out var error);

        // Assert
        success.Should().BeTrue();
        error.Should().BeNull();
        updated!.Quantity.Should().Be(40);   // 50 - 10
    }

    [Fact]
    public void TryReduceStock_SufficientStock_QuantityIsPersistedToDisk()
    {
        // Arrange
        var service = CreateService();
        var medicine = service.Add(SampleRequest(quantity: 50));

        // Act
        service.TryReduceStock(medicine.Id, 10, out _, out _);

        // Assert — read back from the store to confirm it was saved
        var fromStore = service.GetById(medicine.Id);
        fromStore!.Quantity.Should().Be(40);
    }

    [Fact]
    public void TryReduceStock_ExactStock_ReturnsTrueAndSetsQuantityToZero()
    {
        // Arrange
        var service = CreateService();
        var medicine = service.Add(SampleRequest(quantity: 10));

        // Act
        var success = service.TryReduceStock(medicine.Id, 10, out var updated, out var error);

        // Assert
        success.Should().BeTrue();
        updated!.Quantity.Should().Be(0);
    }

    [Fact]
    public void TryReduceStock_InsufficientStock_ReturnsFalseWithMessage()
    {
        // Arrange
        var service = CreateService();
        var medicine = service.Add(SampleRequest("Vitamin C 500mg", quantity: 3));

        // Act
        var success = service.TryReduceStock(medicine.Id, 10, out _, out var error);

        // Assert
        success.Should().BeFalse();
        error.Should().Contain("Insufficient stock");
        error.Should().Contain("Available: 3");
        error.Should().Contain("requested: 10");
    }

    [Fact]
    public void TryReduceStock_InsufficientStock_DoesNotChangeQuantity()
    {
        // Arrange
        var service = CreateService();
        var medicine = service.Add(SampleRequest(quantity: 3));

        // Act
        service.TryReduceStock(medicine.Id, 10, out _, out _);

        // Assert — quantity must be unchanged
        var fromStore = service.GetById(medicine.Id);
        fromStore!.Quantity.Should().Be(3);
    }

    [Fact]
    public void TryReduceStock_MedicineNotFound_ReturnsFalseWithMessage()
    {
        // Arrange
        var service = CreateService();

        // Act
        var success = service.TryReduceStock(999, 1, out var medicine, out var error);

        // Assert
        success.Should().BeFalse();
        medicine.Should().BeNull();
        error.Should().Contain("999");
    }
}