using System.Text.Json;
using System.Text.RegularExpressions;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.Services.EasyCars;

/// <summary>
/// Service for mapping EasyCars StockItem data to Vehicle entity
/// Handles duplicate detection, field mapping, data type conversions, and raw data storage
/// </summary>
public class EasyCarsStockMapper : IEasyCarsStockMapper
{
    private readonly ILogger<EasyCarsStockMapper> _logger;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IEasyCarsStockDataRepository _stockDataRepository;
    private readonly IImageDownloadService _imageDownloadService;

    public EasyCarsStockMapper(
        ILogger<EasyCarsStockMapper> logger,
        IVehicleRepository vehicleRepository,
        IEasyCarsStockDataRepository stockDataRepository,
        IImageDownloadService imageDownloadService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _stockDataRepository = stockDataRepository ?? throw new ArgumentNullException(nameof(stockDataRepository));
        _imageDownloadService = imageDownloadService ?? throw new ArgumentNullException(nameof(imageDownloadService));
    }

    public async Task<StockMapResult> MapToVehicleAsync(StockItem stockItem, int dealershipId, CancellationToken cancellationToken = default)
    {
        if (stockItem == null)
            throw new ArgumentNullException(nameof(stockItem));

        if (dealershipId <= 0)
            throw new ArgumentException("Invalid dealership ID", nameof(dealershipId));

        _logger.LogInformation(
            "Mapping StockItem to Vehicle (StockNumber: {StockNumber}, VIN: {VIN}, Make: {Make}, Model: {Model})",
            stockItem.StockNumber, stockItem.VIN, stockItem.Make, stockItem.Model);

        try
        {
            // Try to find existing vehicle by VIN
            Vehicle? vehicle = null;
            var vin = stockItem.VIN?.Trim();

            if (!string.IsNullOrEmpty(vin))
            {
                vehicle = await _vehicleRepository.FindByVinAsync(vin, cancellationToken);

                if (vehicle != null && vehicle.DataSource != DataSource.EasyCars)
                {
                    _logger.LogWarning(
                        "Vehicle with VIN {VIN} exists as {Source} entry, skipping EasyCars sync",
                        vin, vehicle.DataSource);
                    return new StockMapResult(vehicle, 0, 0);
                }
            }

            // If VIN not found, try StockNumber
            if (vehicle == null && !string.IsNullOrEmpty(stockItem.StockNumber))
            {
                vehicle = await _vehicleRepository.FindByStockNumberAsync(stockItem.StockNumber, dealershipId, cancellationToken);

                if (vehicle != null && vehicle.DataSource != DataSource.EasyCars)
                {
                    _logger.LogWarning(
                        "Vehicle with StockNumber {StockNumber} exists as {Source} entry, skipping EasyCars sync",
                        stockItem.StockNumber, vehicle.DataSource);
                    return new StockMapResult(vehicle, 0, 0);
                }
            }

            // Create or update vehicle
            if (vehicle == null)
            {
                vehicle = CreateVehicleFromStockItem(stockItem, dealershipId);
                await _vehicleRepository.AddAsync(vehicle, cancellationToken);
                _logger.LogInformation("Created new vehicle {VehicleId} from EasyCars sync", vehicle.Id);
            }
            else
            {
                UpdateVehicleFields(vehicle, stockItem);
                await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
                _logger.LogInformation("Updated existing vehicle {VehicleId} from EasyCars sync", vehicle.Id);
            }

            // Store raw JSON data
            var rawJson = JsonSerializer.Serialize(stockItem);
            await _stockDataRepository.UpsertAsync(vehicle.Id, rawJson, "1.0", cancellationToken);

            // AC1, AC2, AC4: Sync images from EasyCars; track counts for AC8
            var imagesDownloaded = 0;
            var imagesFailed = 0;
            var imageUrls = stockItem.ImageURLs?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList() ?? new List<string>();
            if (imageUrls.Count > 0)
            {
                var cloudinaryUrls = await _imageDownloadService.DownloadAndStoreImagesAsync(
                    imageUrls, vehicle.Id, cancellationToken);
                imagesDownloaded = cloudinaryUrls.Count;
                imagesFailed = imageUrls.Count - cloudinaryUrls.Count;
                if (cloudinaryUrls.Count > 0)
                {
                    vehicle.Update(vehicle.Make, vehicle.Model, vehicle.Year, vehicle.Price,
                        vehicle.Mileage, vehicle.Condition, vehicle.Status,
                        vehicle.Title, vehicle.Description, cloudinaryUrls);
                    await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
                    _logger.LogInformation("Updated {Count} images for vehicle {VehicleId}", cloudinaryUrls.Count, vehicle.Id);
                }
            }

            return new StockMapResult(vehicle, imagesDownloaded, imagesFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map StockItem {StockNumber} to Vehicle", stockItem.StockNumber);
            throw;
        }
    }

    private Vehicle CreateVehicleFromStockItem(StockItem stockItem, int dealershipId)
    {
        var make = GetStringOrDefault(stockItem.Make, "Unknown");
        var model = GetStringOrDefault(stockItem.Model, "Unknown");
        var year = GetValidYear(stockItem.Year);
        var price = ParseDecimal(stockItem.Price?.ToString());
        var mileage = Math.Max(0, stockItem.Odometer ?? 0);
        var condition = ConvertToVehicleCondition(stockItem.StockType);
        var title = $"{year} {make} {model}";
        var description = GetStringOrDefault(stockItem.Description, "No description available");

        var vehicle = Vehicle.Create(
            dealershipId,
            make,
            model,
            year,
            price,
            mileage,
            condition,
            VehicleStatus.Active,
            title,
            description);

        // Set EasyCars-specific fields
        vehicle.UpdateEasyCarsData(
            stockItem.StockNumber,
            stockItem.YardCode,
            stockItem.VIN?.Trim(),
            null, // raw data stored separately
            stockItem.Colour,
            null, // interior color not in API
            stockItem.Body,
            stockItem.FuelType,
            stockItem.Transmission,
            string.IsNullOrEmpty(stockItem.EngineCapacity) ? stockItem.EngineSize?.ToString() : stockItem.EngineCapacity,
            stockItem.Doors > 0 ? stockItem.Doors : 4,
            ToFeaturesJson(stockItem.KeyFeatures));

        return vehicle;
    }

    private void UpdateVehicleFields(Vehicle vehicle, StockItem stockItem)
    {
        var make = GetStringOrDefault(stockItem.Make, "Unknown");
        var model = GetStringOrDefault(stockItem.Model, "Unknown");
        var year = GetValidYear(stockItem.Year);
        var price = ParseDecimal(stockItem.Price?.ToString());
        var mileage = Math.Max(0, stockItem.Odometer ?? 0);
        var condition = ConvertToVehicleCondition(stockItem.StockType);
        var title = $"{year} {make} {model}";
        var description = GetStringOrDefault(stockItem.Description, "No description available");

        vehicle.Update(
            make,
            model,
            year,
            price,
            mileage,
            condition,
            VehicleStatus.Active,
            title,
            description);

        // Update EasyCars-specific fields
        vehicle.UpdateEasyCarsData(
            stockItem.StockNumber,
            stockItem.YardCode,
            stockItem.VIN?.Trim(),
            null, // raw data stored separately
            stockItem.Colour,
            null, // interior color not in API
            stockItem.Body,
            stockItem.FuelType,
            stockItem.Transmission,
            string.IsNullOrEmpty(stockItem.EngineCapacity) ? stockItem.EngineSize?.ToString() : stockItem.EngineCapacity,
            stockItem.Doors > 0 ? stockItem.Doors : 4,
            ToFeaturesJson(stockItem.KeyFeatures));
    }

    private VehicleCondition ConvertToVehicleCondition(string? stockType)
    {
        if (string.IsNullOrEmpty(stockType))
            return VehicleCondition.Used;

        return stockType.ToLowerInvariant() switch
        {
            "new" => VehicleCondition.New,
            "used" => VehicleCondition.Used,
            "demo" => VehicleCondition.Used, // Demo cars are considered used
            _ => VehicleCondition.Used
        };
    }

    private string GetStringOrDefault(string? value, string defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (defaultValue != value)
            {
                _logger.LogWarning("Using default value '{DefaultValue}' for null/empty field", defaultValue);
            }
            return defaultValue;
        }

        return value;
    }

    private int GetValidYear(int year)
    {
        if (year < 1900 || year > 2100)
        {
            var currentYear = DateTime.UtcNow.Year;
            _logger.LogWarning("Invalid year {Year}, using {DefaultYear}", year, currentYear - 1);
            return currentYear - 1;
        }

        return year;
    }

    private static string? ToFeaturesJson(string? keyFeatures)
    {
        if (string.IsNullOrWhiteSpace(keyFeatures))
            return null;

        var items = keyFeatures
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        return items.Count == 0 ? null : JsonSerializer.Serialize(items);
    }

    private decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        // Remove currency symbols, commas, spaces
        var cleaned = Regex.Replace(value, @"[^\d.]", "");

        if (decimal.TryParse(cleaned, out var result))
            return Math.Max(0, result);

        _logger.LogWarning("Failed to parse decimal value '{Value}', using 0", value);
        return 0;
    }
}
