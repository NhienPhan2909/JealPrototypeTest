using Hangfire;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.BackgroundJobs;

public class StockSyncBackgroundJob
{
    private readonly IEasyCarsStockSyncService _syncService;
    private readonly IEasyCarsCredentialRepository _credentialRepo;
    private readonly IDealershipSettingsRepository _settingsRepo;
    private readonly ISystemSettingsRepository _systemSettingsRepo;
    private readonly ILogger<StockSyncBackgroundJob> _logger;
    
    public StockSyncBackgroundJob(
        IEasyCarsStockSyncService syncService,
        IEasyCarsCredentialRepository credentialRepo,
        IDealershipSettingsRepository settingsRepo,
        ISystemSettingsRepository systemSettingsRepo,
        ILogger<StockSyncBackgroundJob> logger)
    {
        _syncService = syncService;
        _credentialRepo = credentialRepo;
        _settingsRepo = settingsRepo;
        _systemSettingsRepo = systemSettingsRepo;
        _logger = logger;
    }
    
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stock synchronization job started at {Timestamp}", DateTime.UtcNow);
        
        try
        {
            // Check if global sync is enabled
            var syncEnabled = await _systemSettingsRepo.GetBoolValueAsync("easycar_sync_enabled", true, cancellationToken);
            if (!syncEnabled)
            {
                _logger.LogInformation("Stock synchronization globally disabled, skipping job execution");
                return;
            }
            
            // Get list of eligible dealerships
            var eligibleDealershipIds = await GetEligibleDealershipsAsync(cancellationToken);
            
            if (eligibleDealershipIds.Count == 0)
            {
                _logger.LogInformation("No dealerships eligible for synchronization");
                return;
            }
            
            _logger.LogInformation("Found {Count} dealerships eligible for synchronization", eligibleDealershipIds.Count);
            
            var results = new List<(int DealershipId, bool Success, string? Error)>();
            
            // Process each dealership sequentially
            foreach (var dealershipId in eligibleDealershipIds)
            {
                try
                {
                    _logger.LogInformation("Starting sync for dealership {DealershipId}", dealershipId);
                    
                    var result = await _syncService.SyncStockAsync(dealershipId, cancellationToken);
                    
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("✅ Dealership {DealershipId} synced successfully: {Count} vehicles processed, {Succeeded} succeeded, {Failed} failed",
                            dealershipId, result.ItemsProcessed, result.ItemsSucceeded, result.ItemsFailed);
                        results.Add((dealershipId, true, null));
                    }
                    else if (result.IsPartialSuccess)
                    {
                        _logger.LogWarning("⚠️ Dealership {DealershipId} sync partially successful: {Succeeded} succeeded, {Failed} failed. Errors: {Errors}",
                            dealershipId, result.ItemsSucceeded, result.ItemsFailed, string.Join(", ", result.Errors));
                        results.Add((dealershipId, true, null)); // Partial success still counts as processed
                    }
                    else
                    {
                        _logger.LogError("❌ Dealership {DealershipId} sync failed: {Errors}",
                            dealershipId, string.Join(", ", result.Errors));
                        results.Add((dealershipId, false, string.Join(", ", result.Errors)));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Exception during sync for dealership {DealershipId}", dealershipId);
                    results.Add((dealershipId, false, ex.Message));
                }
            }
            
            // Log summary
            var succeeded = results.Count(r => r.Success);
            var failed = results.Count(r => !r.Success);
            
            _logger.LogInformation("Stock synchronization job completed: {Total} total, {Succeeded} succeeded, {Failed} failed",
                results.Count, succeeded, failed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in stock synchronization job");
            throw;
        }
    }
    
    private async Task<List<int>> GetEligibleDealershipsAsync(CancellationToken cancellationToken)
    {
        // Get dealerships with auto-sync enabled
        var dealershipIds = await _settingsRepo.GetDealershipsWithAutoSyncEnabledAsync(cancellationToken);
        
        // Filter to only those with active credentials
        var eligible = new List<int>();
        foreach (var id in dealershipIds)
        {
            var credential = await _credentialRepo.GetByDealershipIdAsync(id);
            if (credential != null)
            {
                eligible.Add(id);
            }
        }
        
        return eligible;
    }
    
    /// <summary>
    /// Executes manual sync for a specific dealership (Story 2.5)
    /// Triggered by admin interface, bypasses auto-sync checks
    /// </summary>
    [AutomaticRetry(Attempts = 1)]
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ExecuteManualSyncAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Manual stock synchronization started for dealership {DealershipId} at {Timestamp}", 
            dealershipId, DateTime.UtcNow);
        
        try
        {
            var result = await _syncService.SyncStockAsync(dealershipId, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Manual sync completed successfully for dealership {DealershipId}: {Count} vehicles processed, {Succeeded} succeeded, {Failed} failed",
                    dealershipId, result.ItemsProcessed, result.ItemsSucceeded, result.ItemsFailed);
            }
            else if (result.IsPartialSuccess)
            {
                _logger.LogWarning("⚠️ Manual sync partially successful for dealership {DealershipId}: {Succeeded} succeeded, {Failed} failed. Errors: {Errors}",
                    dealershipId, result.ItemsSucceeded, result.ItemsFailed, string.Join(", ", result.Errors));
            }
            else
            {
                _logger.LogError("❌ Manual sync failed for dealership {DealershipId}: {Errors}",
                    dealershipId, string.Join(", ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception during manual sync for dealership {DealershipId}", dealershipId);
            throw;
        }
    }
}
