namespace JealPrototype.Application.DTOs.EasyCars;

using VehicleEntity = JealPrototype.Domain.Entities.Vehicle;

/// <summary>
/// Result of mapping a single EasyCars StockItem, including image sync statistics.
/// Used to propagate image counts from EasyCarsStockMapper up to EasyCarsStockSyncService
/// so they can be aggregated into the overall SyncResult (Story 2.6 AC8).
/// </summary>
public record StockMapResult(VehicleEntity Vehicle, int ImagesDownloaded, int ImagesFailed);
