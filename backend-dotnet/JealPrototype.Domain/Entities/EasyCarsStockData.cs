namespace JealPrototype.Domain.Entities;

/// <summary>
/// Stores raw JSON data from EasyCars API for audit trail and troubleshooting
/// </summary>
public class EasyCarsStockData : BaseEntity
{
    public int VehicleId { get; private set; }
    public string StockItemJson { get; private set; } = string.Empty;
    public DateTime SyncedAt { get; private set; }
    public string ApiVersion { get; private set; } = "1.0";

    public Vehicle Vehicle { get; private set; } = null!;

    private EasyCarsStockData() { }

    public static EasyCarsStockData Create(int vehicleId, string stockItemJson, string apiVersion = "1.0")
    {
        if (vehicleId <= 0)
            throw new ArgumentException("Invalid vehicle ID", nameof(vehicleId));

        if (string.IsNullOrWhiteSpace(stockItemJson))
            throw new ArgumentException("Stock item JSON is required", nameof(stockItemJson));

        return new EasyCarsStockData
        {
            VehicleId = vehicleId,
            StockItemJson = stockItemJson,
            SyncedAt = DateTime.UtcNow,
            ApiVersion = apiVersion
        };
    }

    public void UpdateStockData(string stockItemJson)
    {
        if (string.IsNullOrWhiteSpace(stockItemJson))
            throw new ArgumentException("Stock item JSON is required", nameof(stockItemJson));

        StockItemJson = stockItemJson;
        SyncedAt = DateTime.UtcNow;
    }
}
