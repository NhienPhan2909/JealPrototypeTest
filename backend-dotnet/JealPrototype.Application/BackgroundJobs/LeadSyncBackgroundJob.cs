using Hangfire;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.BackgroundJobs;

public class LeadSyncBackgroundJob
{
    private readonly IEasyCarsLeadSyncService _syncService;
    private readonly IEasyCarsCredentialRepository _credentialRepo;
    private readonly IDealershipSettingsRepository _settingsRepo;
    private readonly ILogger<LeadSyncBackgroundJob> _logger;

    public LeadSyncBackgroundJob(
        IEasyCarsLeadSyncService syncService,
        IEasyCarsCredentialRepository credentialRepo,
        IDealershipSettingsRepository settingsRepo,
        ILogger<LeadSyncBackgroundJob> logger)
    {
        _syncService = syncService;
        _credentialRepo = credentialRepo;
        _settingsRepo = settingsRepo;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task SyncLeadAsync(int leadId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Lead sync job started for lead {LeadId} at {Timestamp}", leadId, DateTime.UtcNow);
        try
        {
            var result = await _syncService.SyncLeadToEasyCarsAsync(leadId, cancellationToken);
            if (result.IsSuccess)
                _logger.LogInformation("✅ Lead {LeadId} synced to EasyCars successfully: {ItemsSucceeded} items", leadId, result.ItemsSucceeded);
            else
                _logger.LogError("❌ Lead {LeadId} sync failed: {Errors}", leadId, string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in lead sync job for lead {LeadId}", leadId);
            throw; // Allow Hangfire AutomaticRetry to retry
        }
    }

    [AutomaticRetry(Attempts = 1)]
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ExecuteManualLeadSyncAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Manual inbound lead sync job started for dealership {DealershipId} at {Timestamp}", dealershipId, DateTime.UtcNow);
        try
        {
            var result = await _syncService.SyncLeadsFromEasyCarsAsync(dealershipId, cancellationToken);
            if (result.IsSuccess)
                _logger.LogInformation("✅ Manual inbound lead sync for dealership {DealershipId} succeeded: {Succeeded} leads", dealershipId, result.ItemsSucceeded);
            else if (result.IsPartialSuccess)
                _logger.LogWarning("⚠️ Manual inbound lead sync for dealership {DealershipId} partially succeeded: {Succeeded} ok, {Failed} failed", dealershipId, result.ItemsSucceeded, result.ItemsFailed);
            else
                _logger.LogError("❌ Manual inbound lead sync for dealership {DealershipId} failed: {Errors}", dealershipId, string.Join(", ", result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in manual inbound lead sync job for dealership {DealershipId}", dealershipId);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ExecuteInboundSyncAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Inbound lead sync job started at {Timestamp}", DateTime.UtcNow);

        try
        {
            var dealershipIds = await _settingsRepo.GetDealershipsWithAutoSyncEnabledAsync(cancellationToken);

            if (dealershipIds.Count == 0)
            {
                _logger.LogInformation("No dealerships eligible for inbound lead sync");
                return;
            }

            _logger.LogInformation("Found {Count} dealerships eligible for inbound lead sync", dealershipIds.Count);

            foreach (var dealershipId in dealershipIds)
            {
                var hasCredentials = await _credentialRepo.ExistsForDealershipAsync(dealershipId);
                if (!hasCredentials)
                {
                    _logger.LogInformation("Skipping dealership {DealershipId} — no EasyCars credentials", dealershipId);
                    continue;
                }

                try
                {
                    var result = await _syncService.SyncLeadsFromEasyCarsAsync(dealershipId, cancellationToken);

                    if (result.IsSuccess)
                        _logger.LogInformation("✅ Dealership {DealershipId} inbound lead sync succeeded: {Succeeded} leads", dealershipId, result.ItemsSucceeded);
                    else if (result.IsPartialSuccess)
                        _logger.LogWarning("⚠️ Dealership {DealershipId} inbound lead sync partially succeeded: {Succeeded} ok, {Failed} failed", dealershipId, result.ItemsSucceeded, result.ItemsFailed);
                    else
                        _logger.LogError("❌ Dealership {DealershipId} inbound lead sync failed: {Errors}", dealershipId, string.Join(", ", result.Errors));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Exception during inbound lead sync for dealership {DealershipId}", dealershipId);
                }
            }

            _logger.LogInformation("Inbound lead sync job completed at {Timestamp}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in inbound lead sync job");
            throw;
        }
    }

    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ExecuteStatusSyncAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Lead status sync job started at {Timestamp}", DateTime.UtcNow);

        try
        {
            var dealershipIds = await _settingsRepo.GetDealershipsWithAutoSyncEnabledAsync(cancellationToken);

            if (dealershipIds.Count == 0)
            {
                _logger.LogInformation("No dealerships eligible for lead status sync");
                return;
            }

            foreach (var dealershipId in dealershipIds)
            {
                var hasCredentials = await _credentialRepo.ExistsForDealershipAsync(dealershipId);
                if (!hasCredentials)
                {
                    _logger.LogInformation("Skipping dealership {DealershipId} — no EasyCars credentials", dealershipId);
                    continue;
                }

                try
                {
                    var result = await _syncService.SyncLeadStatusesFromEasyCarsAsync(dealershipId, cancellationToken);
                    if (result.IsSuccess)
                        _logger.LogInformation("✅ Dealership {DealershipId} status sync succeeded: {Succeeded} leads", dealershipId, result.ItemsSucceeded);
                    else if (result.IsPartialSuccess)
                        _logger.LogWarning("⚠️ Dealership {DealershipId} status sync partial: {Succeeded} ok, {Failed} failed", dealershipId, result.ItemsSucceeded, result.ItemsFailed);
                    else
                        _logger.LogError("❌ Dealership {DealershipId} status sync failed: {Errors}", dealershipId, string.Join(", ", result.Errors));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception during lead status sync for dealership {DealershipId}", dealershipId);
                }
            }

            _logger.LogInformation("Lead status sync job completed at {Timestamp}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in lead status sync job");
            throw;
        }
    }
}
