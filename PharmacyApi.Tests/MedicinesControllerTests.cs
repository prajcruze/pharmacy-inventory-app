using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PharmacyApi.Controllers;
using PharmacyApi.Models;
using PharmacyApi.Services;

namespace PharmacyApi.Tests;

public class MedicinesControllerTests
{
    // shared mock and controller — created fresh before each test
    private readonly Mock<IMedicineService> _mockService;
    private readonly MedicinesController _controller;

    public MedicinesControllerTests()
    {
        _mockService = new Mock<IMedicineService>();
        _controller = new MedicinesController(_mockService.Object);
    }

    // -----------------------------------------------------------------------
    // GetAll
    // -----------------------------------------------------------------------

    [Fact]
    public void GetAll_NoSearch_ReturnsOkWithAllMedicines()
    {
        // Arrange
        var medicines = new List<Medicine>
        {
            new() { Id = 1, FullName = "Paracetamol 500mg", Brand = "Crocin",
                    ExpiryDate = DateTime.Now.AddYears(1), Quantity = 50, Price = 25.50m },
            new() { Id = 2, FullName = "Ibuprofen 400mg", Brand = "Brufen",
                    ExpiryDate = DateTime.Now.AddYears(2), Quantity = 30, Price = 42.00m }
        };
        _mockService.Setup(s => s.GetAll()).Returns(medicines);

        // Act
        var result = _controller.GetAll(null);

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);

        var returned = ok.Value as IEnumerable<Medicine>;
        returned.Should().HaveCount(2);
    }

    [Fact]
    public void GetAll_WithSearch_CallsSearchNotGetAll()
    {
        // Arrange
        var filtered = new List<Medicine>
        {
            new() { Id = 1, FullName = "Paracetamol 500mg", Brand = "Crocin",
                    ExpiryDate = DateTime.Now.AddYears(1), Quantity = 50, Price = 25.50m }
        };
        _mockService.Setup(s => s.Search("para")).Returns(filtered);

        // Act
        var result = _controller.GetAll("para");

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);

        // Verify Search was called, NOT GetAll
        _mockService.Verify(s => s.Search("para"), Times.Once);
        _mockService.Verify(s => s.GetAll(), Times.Never);
    }

    [Fact]
    public void GetAll_EmptyList_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockService.Setup(s => s.GetAll()).Returns(new List<Medicine>());

        // Act
        var result = _controller.GetAll(null);

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var returned = ok!.Value as IEnumerable<Medicine>;
        returned.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // GetById
    // -----------------------------------------------------------------------

    [Fact]
    public void GetById_MedicineExists_ReturnsOk()
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
        _mockService.Setup(s => s.GetById(1)).Returns(medicine);

        // Act
        var result = _controller.GetById(1);

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);

        var returned = ok.Value as Medicine;
        returned!.Id.Should().Be(1);
        returned.FullName.Should().Be("Paracetamol 500mg");
    }

    [Fact]
    public void GetById_MedicineNotFound_Returns404()
    {
        // Arrange
        _mockService.Setup(s => s.GetById(999)).Returns((Medicine?)null);

        // Act
        var result = _controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // -----------------------------------------------------------------------
    // Create
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateMedicineRequest
        {
            FullName = "Metformin 500mg",
            Brand = "Glucophage",
            ExpiryDate = DateTime.Now.AddYears(2),
            Quantity = 60,
            Price = 35.50m
        };

        var created = new Medicine
        {
            Id = 1,
            FullName = request.FullName,
            Brand = request.Brand,
            ExpiryDate = request.ExpiryDate,
            Quantity = request.Quantity,
            Price = request.Price
        };

        _mockService.Setup(s => s.Add(request)).Returns(created);

        // Act
        var result = _controller.Create(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);

        var returned = createdResult.Value as Medicine;
        returned!.Id.Should().Be(1);
        returned.FullName.Should().Be("Metformin 500mg");
    }
}