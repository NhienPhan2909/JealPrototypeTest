using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Interfaces;
using JealPrototype.Application.Services.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace JealPrototype.Tests.Unit.Services.EasyCars;

/// <summary>
/// Unit tests for Story 2.2: EasyCarsStockMapper
/// Tests field mapping, duplicate detection, data conversions, and error handling
/// </summary>
public class EasyCarsStockMapperTests
{
    private readonly Mock<ILogger<EasyCarsStockMapper>> _mockLogger;
    private readonly Mock<IVehicleRepository> _mockVehicleRepo;
    private readonly Mock<IEasyCarsStockDataRepository> _mockStockDataRepo;
    private readonly Mock<IImageDownloadService> _mockImageDownloadService;
    private readonly EasyCarsStockMapper _sut;

    public EasyCarsStockMapperTests()
    {
        _mockLogger = new Mock<ILogger<EasyCarsStockMapper>>();
        _mockVehicleRepo = new Mock<IVehicleRepository>();
        _mockStockDataRepo = new Mock<IEasyCarsStockDataRepository>();
        _mockImageDownloadService = new Mock<IImageDownloadService>();
        _mockImageDownloadService
            .Setup(s => s.DownloadAndStoreImagesAsync(It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());
        _sut = new EasyCarsStockMapper(_mockLogger.Object, _mockVehicleRepo.Object, _mockStockDataRepo.Object, _mockImageDownloadService.Object);
    }

    // ==== Field Mapping Accuracy Tests (10 tests) ====

    [Fact]
    public async Task MapToVehicleAsync_WithCompleteData_MapsAllCriticalFields()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Honda", result.Make);
        Assert.Equal("Accord", result.Model);
        Assert.Equal(2023, result.Year);
        Assert.Equal(35000, result.Price);
        Assert.Equal(25000, result.Mileage);
        Assert.Equal(VehicleCondition.Used, result.Condition);
        Assert.Equal(VehicleStatus.Active, result.Status);
        Assert.Equal("2023 Honda Accord", result.Title);
        Assert.Equal("Excellent condition sedan", result.Description);
        Assert.Equal(DataSource.EasyCars, result.DataSource);
    }

    [Fact]
    public async Task MapToVehicleAsync_WithNullMake_UsesDefaultUnknown()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        stockItem.Make = null!;
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.Equal("Unknown", result.Make);
    }

    [Fact]
    public async Task MapToVehicleAsync_WithInvalidYear_UsesCurrentYearMinusOne()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        stockItem.Year = 1800; // Invalid year
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        var expectedYear = DateTime.UtcNow.Year - 1;
        Assert.Equal(expectedYear, result.Year);
    }

    [Fact]
    public async Task MapToVehicleAsync_WithPriceString_ConvertsToDecimal()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        stockItem.Price = 45000.50m;
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.Equal(45000.50m, result.Price);
    }

    [Fact]
    public async Task MapToVehicleAsync_WithStockType_ConvertsToEnum()
    {
        // Arrange - Test "New"
        var stockItem = CreateValidStockItem();
        stockItem.StockType = "New";
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.Equal(VehicleCondition.New, result.Condition);
    }

    [Fact]
    public async Task MapToVehicleAsync_SetsDealershipIdCorrectly()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 42)).Vehicle;

        // Assert
        Assert.Equal(42, result.DealershipId);
    }

    [Fact]
    public async Task MapToVehicleAsync_SetsSourceAsEasyCars()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.Equal(DataSource.EasyCars, result.DataSource);
    }

    [Fact]
    public async Task MapToVehicleAsync_WithEmptyDescription_UsesDefault()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        stockItem.Description = "";
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.Equal("No description available", result.Description);
    }

    [Fact]
    public async Task MapToVehicleAsync_MapsEasyCarsSpecificFields()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.Equal("ST12345", result.EasyCarsStockNumber);
        Assert.Equal("YARD-01", result.EasyCarsYardCode);
        Assert.Equal("1HGBH41JXMN109186", result.EasyCarsVIN);
        Assert.Equal("Silver", result.ExteriorColor);
        Assert.Equal("Sedan", result.Body);
        Assert.Equal("Petrol", result.FuelType);
        Assert.Equal("Automatic", result.GearType);
    }

    [Fact]
    public async Task MapToVehicleAsync_WithNegativeMileage_UsesZero()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        stockItem.Odometer = -1000;
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.Equal(0, result.Mileage);
    }

    // ==== Duplicate Detection Tests (6 tests) ====

    [Fact]
    public async Task MapToVehicleAsync_WithExistingVIN_UpdatesExistingVehicle()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        var existingVehicle = Vehicle.Create(1, "Toyota", "Camry", 2022, 30000, 15000, VehicleCondition.Used, VehicleStatus.Active, "2022 Toyota Camry");
        existingVehicle.UpdateEasyCarsData("OLD123", "YARD-01", stockItem.VIN, null);

        _mockVehicleRepo.Setup(r => r.FindByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVehicle);
        _mockVehicleRepo.Setup(r => r.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockStockDataRepo.Setup(r => r.UpsertAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EasyCarsStockData.Create(1, "{\"test\":\"data\"}", "1.0"));

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.Equal("Honda", result.Make); // Updated from Toyota
        Assert.Equal("Accord", result.Model); // Updated from Camry
        _mockVehicleRepo.Verify(r => r.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockVehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MapToVehicleAsync_WithExistingStockNumber_UpdatesExistingVehicle()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        stockItem.VIN = ""; // No VIN

        var existingVehicle = Vehicle.Create(1, "Toyota", "Camry", 2022, 30000, 15000, VehicleCondition.Used, VehicleStatus.Active, "2022 Toyota Camry");
        existingVehicle.UpdateEasyCarsData(stockItem.StockNumber, "YARD-01", "", null);

        _mockVehicleRepo.Setup(r => r.FindByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);
        _mockVehicleRepo.Setup(r => r.FindByStockNumberAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVehicle);
        _mockVehicleRepo.Setup(r => r.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockStockDataRepo.Setup(r => r.UpsertAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EasyCarsStockData.Create(1, "{\"test\":\"data\"}", "1.0"));

        // Act
        var result = await _sut.MapToVehicleAsync(stockItem, dealershipId: 1);

        // Assert
        _mockVehicleRepo.Verify(r => r.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockVehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MapToVehicleAsync_WithNoMatch_CreatesNewVehicle()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        SetupNoExistingVehicle();

        // Act
        var result = await _sut.MapToVehicleAsync(stockItem, dealershipId: 1);

        // Assert
        _mockVehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockVehicleRepo.Verify(r => r.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MapToVehicleAsync_WithManualEntry_SkipsUpdate()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        var manualVehicle = Vehicle.Create(1, "Honda", "Accord", 2023, 35000, 25000, VehicleCondition.Used, VehicleStatus.Active, "2023 Honda Accord");
        // manualVehicle.DataSource defaults to Manual

        _mockVehicleRepo.Setup(r => r.FindByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(manualVehicle);

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.Equal(DataSource.Manual, result.DataSource); // Not changed
        _mockVehicleRepo.Verify(r => r.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockVehicleRepo.Verify(r => r.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MapToVehicleAsync_WithVINAndStockNumberMatch_PrefersVIN()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        var vehicleByVIN = Vehicle.Create(1, "Honda", "Accord", 2023, 35000, 25000, VehicleCondition.Used, VehicleStatus.Active, "2023 Honda Accord");
        vehicleByVIN.UpdateEasyCarsData("ST12345", "YARD-01", stockItem.VIN, null);

        _mockVehicleRepo.Setup(r => r.FindByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleByVIN);
        _mockVehicleRepo.Setup(r => r.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockStockDataRepo.Setup(r => r.UpsertAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EasyCarsStockData.Create(1, "{\"test\":\"data\"}", "1.0"));

        // Act
        var result = await _sut.MapToVehicleAsync(stockItem, dealershipId: 1);

        // Assert
        _mockVehicleRepo.Verify(r => r.FindByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockVehicleRepo.Verify(r => r.FindByStockNumberAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MapToVehicleAsync_SetsLastSyncedAt()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        SetupNoExistingVehicle();
        var beforeSync = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert
        Assert.NotNull(result.LastSyncedFromEasyCars);
        Assert.True(result.LastSyncedFromEasyCars >= beforeSync);
        Assert.True(result.LastSyncedFromEasyCars <= DateTime.UtcNow.AddSeconds(1));
    }

    // ==== Raw Data Storage Tests (3 tests) ====

    [Fact]
    public async Task MapToVehicleAsync_StoresRawJsonData()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        SetupNoExistingVehicle();

        // Act
        await _sut.MapToVehicleAsync(stockItem, dealershipId: 1);

        // Assert
        _mockStockDataRepo.Verify(
            r => r.UpsertAsync(
                It.IsAny<int>(),
                It.Is<string>(json => json.Contains("ST12345") && json.Contains("Honda")),
                "1.0",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapToVehicleAsync_UpdatesExistingRawData()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        var existingVehicle = Vehicle.Create(1, "Toyota", "Camry", 2022, 30000, 15000, VehicleCondition.Used, VehicleStatus.Active, "2022 Toyota Camry");
        existingVehicle.UpdateEasyCarsData("ST12345", "YARD-01", stockItem.VIN, null);

        _mockVehicleRepo.Setup(r => r.FindByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVehicle);
        _mockVehicleRepo.Setup(r => r.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockStockDataRepo.Setup(r => r.UpsertAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EasyCarsStockData.Create(1, "{\"test\":\"data\"}", "1.0"));

        // Act
        await _sut.MapToVehicleAsync(stockItem, dealershipId: 1);

        // Assert
        _mockStockDataRepo.Verify(
            r => r.UpsertAsync(It.IsAny<int>(), It.IsAny<string>(), "1.0", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapToVehicleAsync_SetsApiVersion()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        SetupNoExistingVehicle();

        // Act
        await _sut.MapToVehicleAsync(stockItem, dealershipId: 1);

        // Assert
        _mockStockDataRepo.Verify(
            r => r.UpsertAsync(It.IsAny<int>(), It.IsAny<string>(), "1.0", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ==== Error Handling Tests (4 tests) ====

    [Fact]
    public async Task MapToVehicleAsync_WithNullStockItem_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.MapToVehicleAsync(null!, dealershipId: 1));
    }

    [Fact]
    public async Task MapToVehicleAsync_WithInvalidDealershipId_ThrowsArgumentException()
    {
        // Arrange
        var stockItem = CreateValidStockItem();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.MapToVehicleAsync(stockItem, dealershipId: 0));
    }

    [Fact]
    public async Task MapToVehicleAsync_WithDatabaseError_PropagatesException()
    {
        // Arrange
        var stockItem = CreateValidStockItem();
        _mockVehicleRepo.Setup(r => r.FindByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _sut.MapToVehicleAsync(stockItem, dealershipId: 1));
    }

    [Fact]
    public async Task MapToVehicleAsync_WithPartialData_CompletesMapping()
    {
        // Arrange
        var stockItem = new StockItem
        {
            StockNumber = "ST001",
            VIN = "VIN123",
            Make = null!, // Missing
            Model = null!, // Missing
            Year = 0, // Invalid
            Price = 0,
            Odometer = -100, // Invalid
            Description = "" // Empty
        };
        SetupNoExistingVehicle();

        // Act
        var result = (await _sut.MapToVehicleAsync(stockItem, dealershipId: 1)).Vehicle;

        // Assert - Should not throw, uses defaults
        Assert.NotNull(result);
        Assert.Equal("Unknown", result.Make);
        Assert.Equal("Unknown", result.Model);
        Assert.True(result.Year >= 1900 && result.Year <= 2100);
        Assert.Equal(0, result.Mileage);
        Assert.Equal("No description available", result.Description);
    }

    // ==== Helper Methods ====

    private StockItem CreateValidStockItem()
    {
        return new StockItem
        {
            StockNumber = "ST12345",
            VIN = "1HGBH41JXMN109186",
            Make = "Honda",
            Model = "Accord",
            Badge = "VTi-L",
            Year = 2023,
            Body = "Sedan",
            Colour = "Silver",
            Doors = 4,
            Seats = 5,
            Transmission = "Automatic",
            FuelType = "Petrol",
            EngineSize = "2.4L",
            Odometer = 25000,
            Price = 35000,
            Description = "Excellent condition sedan",
            KeyFeatures = "Navigation, Leather, Sunroof",
            Status = "Available",
            StockType = "Used",
            YardCode = "YARD-01"
        };
    }

    private void SetupNoExistingVehicle()
    {
        _mockVehicleRepo.Setup(r => r.FindByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);
        _mockVehicleRepo.Setup(r => r.FindByStockNumberAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);
        _mockVehicleRepo.Setup(r => r.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle v, CancellationToken ct) => v);
        _mockStockDataRepo.Setup(r => r.UpsertAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EasyCarsStockData.Create(1, "{\"test\":\"data\"}", "1.0"));
    }
}
