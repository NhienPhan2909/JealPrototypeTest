using System.Diagnostics;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.Interfaces;
using JealPrototype.Application.Interfaces.Security;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.Services.EasyCars;

/// <summary>
/// Service that orchestrates outbound lead synchronization from local system to EasyCars API.
/// Implements Story 3.3: Outbound Lead Sync.
/// </summary>
public class EasyCarsLeadSyncService : IEasyCarsLeadSyncService
{
    private readonly ILeadRepository _leadRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IEasyCarsCredentialRepository _credentialRepository;
    private readonly ICredentialEncryptionService _encryptionService;
    private readonly IEasyCarsApiClient _apiClient;
    private readonly IEasyCarsLeadMapper _leadMapper;
    private readonly IEasyCarsSyncLogRepository _syncLogRepository;
    private readonly ILogger<EasyCarsLeadSyncService> _logger;
    private readonly ILeadStatusConflictRepository _conflictRepository;
    private readonly ConflictResolutionStrategy _conflictStrategy;

    public EasyCarsLeadSyncService(
        ILeadRepository leadRepository,
        IVehicleRepository vehicleRepository,
        IEasyCarsCredentialRepository credentialRepository,
        ICredentialEncryptionService encryptionService,
        IEasyCarsApiClient apiClient,
        IEasyCarsLeadMapper leadMapper,
        IEasyCarsSyncLogRepository syncLogRepository,
        ILogger<EasyCarsLeadSyncService> logger,
        ILeadStatusConflictRepository conflictRepository,
        IConfiguration configuration)
    {
        _leadRepository = leadRepository;
        _vehicleRepository = vehicleRepository;
        _credentialRepository = credentialRepository;
        _encryptionService = encryptionService;
        _apiClient = apiClient;
        _leadMapper = leadMapper;
        _syncLogRepository = syncLogRepository;
        _logger = logger;
        _conflictRepository = conflictRepository;
        var strategyStr = configuration["EasyCars:LeadStatusConflictResolutionStrategy"] ?? "RemoteWins";
        _conflictStrategy = Enum.TryParse<ConflictResolutionStrategy>(strategyStr, out var parsed)
            ? parsed
            : ConflictResolutionStrategy.RemoteWins;
    }

    public async Task<SyncResult> SyncLeadToEasyCarsAsync(int leadId, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        Lead? lead = null;
        int dealershipId = 0;

        try
        {
            lead = await _leadRepository.GetByIdAsync(leadId, cancellationToken);
            if (lead == null)
            {
                _logger.LogWarning("Lead {LeadId} not found", leadId);
                return SyncResult.Failure($"Lead {leadId} not found");
            }

            dealershipId = lead.DealershipId;

            var credential = await _credentialRepository.GetByDealershipIdAsync(dealershipId);
            if (credential == null)
            {
                _logger.LogWarning("No EasyCars credentials for dealership {DealershipId}", dealershipId);
                sw.Stop();
                var noCredResult = SyncResult.Failure($"No EasyCars credentials for dealership {dealershipId}", sw.ElapsedMilliseconds);
                await TryCreateSyncLogAsync(dealershipId, noCredResult, cancellationToken, "LeadOutbound");
                return noCredResult;
            }

            var clientId = await _encryptionService.DecryptAsync(credential.ClientIdEncrypted);
            var clientSecret = await _encryptionService.DecryptAsync(credential.ClientSecretEncrypted);
            var accountNumber = await _encryptionService.DecryptAsync(credential.AccountNumberEncrypted);
            var accountSecret = await _encryptionService.DecryptAsync(credential.AccountSecretEncrypted);

            Vehicle? vehicle = lead.VehicleId.HasValue
                ? await _vehicleRepository.GetByIdAsync(lead.VehicleId.Value, cancellationToken)
                : null;

            if (!string.IsNullOrEmpty(lead.EasyCarsLeadNumber))
            {
                // UPDATE path (AC7): lead already exists in EasyCars
                var updateReq = _leadMapper.MapToUpdateLeadRequest(
                    lead, lead.EasyCarsLeadNumber, accountNumber, accountSecret, vehicle);
                var updateResp = await _apiClient.UpdateLeadAsync(
                    clientId, clientSecret, credential.Environment, lead.DealershipId, updateReq, cancellationToken);
                lead.UpdateEasyCarsData(
                    updateResp.LeadNumber ?? lead.EasyCarsLeadNumber,
                    lead.EasyCarsCustomerNo,
                    lead.EasyCarsRawData,
                    lead.VehicleInterestType,
                    lead.FinanceInterested,
                    lead.Rating);
            }
            else
            {
                // CREATE path (AC4): new lead, push to EasyCars
                var createReq = _leadMapper.MapToCreateLeadRequest(lead, accountNumber, accountSecret, vehicle);
                var createResp = await _apiClient.CreateLeadAsync(
                    clientId, clientSecret, credential.Environment, lead.DealershipId, createReq, cancellationToken);
                lead.UpdateEasyCarsData(
                    createResp.LeadNumber,
                    createResp.CustomerNo,
                    lead.EasyCarsRawData,
                    lead.VehicleInterestType,
                    lead.FinanceInterested,
                    lead.Rating);
            }

            lead.MarkSyncedToEasyCars(DateTime.UtcNow); // AC5
            await _leadRepository.UpdateAsync(lead, cancellationToken);

            sw.Stop();
            var result = SyncResult.Success(1, sw.ElapsedMilliseconds);
            await CreateSyncLogAsync(dealershipId, result, cancellationToken, "LeadOutbound"); // AC6
            return result;
        }
        catch (EasyCarsTemporaryException ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Transient EasyCars error syncing lead {LeadId}", leadId);
            var failResult = SyncResult.Failure(ex.Message, sw.ElapsedMilliseconds);
            await TryCreateSyncLogAsync(dealershipId, failResult, cancellationToken, "LeadOutbound");
            return failResult;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error syncing lead {LeadId} to EasyCars", leadId);
            var failResult = SyncResult.Failure(ex.Message, sw.ElapsedMilliseconds);
            await TryCreateSyncLogAsync(dealershipId, failResult, cancellationToken, "LeadOutbound");
            return failResult;
        }
    }

    public async Task<SyncResult> SyncLeadsFromEasyCarsAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Starting inbound lead sync for dealership {DealershipId}", dealershipId);

        try
        {
            var credential = await _credentialRepository.GetByDealershipIdAsync(dealershipId);
            if (credential == null)
            {
                _logger.LogWarning("No EasyCars credentials for dealership {DealershipId}", dealershipId);
                sw.Stop();
                var noCredResult = SyncResult.Failure($"No EasyCars credentials for dealership {dealershipId}", sw.ElapsedMilliseconds);
                await TryCreateSyncLogAsync(dealershipId, noCredResult, cancellationToken);
                return noCredResult;
            }

            var clientId = await _encryptionService.DecryptAsync(credential.ClientIdEncrypted);
            var clientSecret = await _encryptionService.DecryptAsync(credential.ClientSecretEncrypted);
            var accountNumber = await _encryptionService.DecryptAsync(credential.AccountNumberEncrypted);
            var accountSecret = await _encryptionService.DecryptAsync(credential.AccountSecretEncrypted);

            var leads = (await _leadRepository.GetLeadsWithEasyCarsNumberAsync(dealershipId, cancellationToken)).ToList();

            if (!leads.Any())
            {
                _logger.LogInformation("No EasyCars leads to sync for dealership {DealershipId}", dealershipId);
                sw.Stop();
                var emptyResult = SyncResult.Success(0, sw.ElapsedMilliseconds);
                await TryCreateSyncLogAsync(dealershipId, emptyResult, cancellationToken);
                return emptyResult;
            }

            _logger.LogInformation("Syncing {Count} leads from EasyCars for dealership {DealershipId}", leads.Count, dealershipId);

            int succeeded = 0, failed = 0;
            var errors = new List<string>();

            foreach (var lead in leads)
            {
                try
                {
                    var response = await _apiClient.GetLeadDetailAsync(
                        clientId, clientSecret, accountNumber, accountSecret,
                        credential.Environment, dealershipId, lead.EasyCarsLeadNumber!, cancellationToken);

                    _leadMapper.UpdateLeadFromResponse(lead, response);
                    _leadRepository.Update(lead);
                    succeeded++;
                }
                catch (Exception ex)
                {
                    failed++;
                    var errMsg = $"Lead {lead.EasyCarsLeadNumber}: {ex.Message}";
                    errors.Add(errMsg);
                    _logger.LogWarning(ex, "Failed to sync lead {LeadNumber} for dealership {DealershipId}", lead.EasyCarsLeadNumber, dealershipId);
                }
            }

            if (succeeded > 0)
                await _leadRepository.SaveChangesAsync(cancellationToken);

            sw.Stop();

            SyncResult result;
            if (failed == 0)
                result = SyncResult.Success(succeeded, sw.ElapsedMilliseconds);
            else if (succeeded == 0)
                result = SyncResult.Failure(succeeded + failed, errors, sw.ElapsedMilliseconds);
            else
                result = SyncResult.PartialSuccess(succeeded, failed, errors, sw.ElapsedMilliseconds);

            await TryCreateSyncLogAsync(dealershipId, result, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Fatal error in inbound lead sync for dealership {DealershipId}", dealershipId);
            var failResult = SyncResult.Failure(ex.Message, sw.ElapsedMilliseconds);
            await TryCreateSyncLogAsync(dealershipId, failResult, cancellationToken);
            return failResult;
        }
    }

    public async Task<SyncResult> SyncLeadStatusToEasyCarsAsync(int leadId, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        int dealershipId = 0;

        try
        {
            var lead = await _leadRepository.GetByIdAsync(leadId, cancellationToken);
            if (lead == null)
            {
                _logger.LogWarning("Lead {LeadId} not found for status sync", leadId);
                return SyncResult.Failure($"Lead {leadId} not found");
            }

            dealershipId = lead.DealershipId;

            if (string.IsNullOrEmpty(lead.EasyCarsLeadNumber))
            {
                _logger.LogInformation("Skipping status sync for lead {LeadId} — no EasyCarsLeadNumber", leadId);
                sw.Stop();
                return SyncResult.Success(0, sw.ElapsedMilliseconds);
            }

            var credential = await _credentialRepository.GetByDealershipIdAsync(dealershipId);
            if (credential == null)
            {
                sw.Stop();
                var noCredResult = SyncResult.Failure($"No EasyCars credentials for dealership {dealershipId}", sw.ElapsedMilliseconds);
                await TryCreateSyncLogAsync(dealershipId, noCredResult, cancellationToken, "LeadStatusOutbound");
                return noCredResult;
            }

            var clientId      = await _encryptionService.DecryptAsync(credential.ClientIdEncrypted);
            var clientSecret  = await _encryptionService.DecryptAsync(credential.ClientSecretEncrypted);
            var accountNumber = await _encryptionService.DecryptAsync(credential.AccountNumberEncrypted);
            var accountSecret = await _encryptionService.DecryptAsync(credential.AccountSecretEncrypted);

            var updateReq = _leadMapper.MapToStatusOnlyUpdateRequest(lead, accountNumber, accountSecret);
            await _apiClient.UpdateLeadAsync(clientId, clientSecret, credential.Environment, dealershipId, updateReq, cancellationToken);

            lead.MarkStatusSyncedToEasyCars(updateReq.LeadStatus!.Value);
            await _leadRepository.UpdateAsync(lead, cancellationToken);

            sw.Stop();
            var result = SyncResult.Success(1, sw.ElapsedMilliseconds);
            await TryCreateSyncLogAsync(dealershipId, result, cancellationToken, "LeadStatusOutbound");
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error pushing status for lead {LeadId} to EasyCars", leadId);
            var failResult = SyncResult.Failure(ex.Message, sw.ElapsedMilliseconds);
            await TryCreateSyncLogAsync(dealershipId, failResult, cancellationToken, "LeadStatusOutbound");
            return failResult;
        }
    }

    public async Task<SyncResult> SyncLeadStatusesFromEasyCarsAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Starting inbound lead status sync for dealership {DealershipId}", dealershipId);

        try
        {
            var credential = await _credentialRepository.GetByDealershipIdAsync(dealershipId);
            if (credential == null)
            {
                _logger.LogWarning("No EasyCars credentials for dealership {DealershipId}", dealershipId);
                sw.Stop();
                var noCredResult = SyncResult.Failure($"No EasyCars credentials for dealership {dealershipId}", sw.ElapsedMilliseconds);
                await TryCreateSyncLogAsync(dealershipId, noCredResult, cancellationToken, "LeadStatus");
                return noCredResult;
            }

            var clientId      = await _encryptionService.DecryptAsync(credential.ClientIdEncrypted);
            var clientSecret  = await _encryptionService.DecryptAsync(credential.ClientSecretEncrypted);
            var accountNumber = await _encryptionService.DecryptAsync(credential.AccountNumberEncrypted);
            var accountSecret = await _encryptionService.DecryptAsync(credential.AccountSecretEncrypted);

            var leads = (await _leadRepository.GetLeadsWithEasyCarsNumberAsync(dealershipId, cancellationToken)).ToList();

            if (!leads.Any())
            {
                sw.Stop();
                var emptyResult = SyncResult.Success(0, sw.ElapsedMilliseconds);
                await TryCreateSyncLogAsync(dealershipId, emptyResult, cancellationToken, "LeadStatus");
                return emptyResult;
            }

            int succeeded = 0, failed = 0;
            var errors = new List<string>();

            foreach (var lead in leads)
            {
                try
                {
                    var response = await _apiClient.GetLeadDetailAsync(
                        clientId, clientSecret, accountNumber, accountSecret,
                        credential.Environment, dealershipId, lead.EasyCarsLeadNumber!, cancellationToken);

                    if (!response.LeadStatus.HasValue)
                    {
                        succeeded++;
                        continue;
                    }

                    var remoteLeadStatus = _leadMapper.MapLeadStatusFromInt(response.LeadStatus.Value);

                    if (lead.Status == remoteLeadStatus)
                    {
                        lead.MarkStatusSyncedToEasyCars(response.LeadStatus.Value);
                        _leadRepository.Update(lead);
                        succeeded++;
                        continue;
                    }

                    await ApplyConflictStrategyAsync(lead, remoteLeadStatus, response.LeadStatus.Value, cancellationToken);
                    _leadRepository.Update(lead);
                    succeeded++;
                }
                catch (Exception ex)
                {
                    failed++;
                    errors.Add($"Lead {lead.EasyCarsLeadNumber}: {ex.Message}");
                    _logger.LogWarning(ex, "Failed to sync status for lead {LeadNumber}", lead.EasyCarsLeadNumber);
                }
            }

            if (succeeded > 0)
                await _leadRepository.SaveChangesAsync(cancellationToken);

            sw.Stop();
            SyncResult result = failed == 0
                ? SyncResult.Success(succeeded, sw.ElapsedMilliseconds)
                : succeeded == 0
                    ? SyncResult.Failure(succeeded + failed, errors, sw.ElapsedMilliseconds)
                    : SyncResult.PartialSuccess(succeeded, failed, errors, sw.ElapsedMilliseconds);

            await TryCreateSyncLogAsync(dealershipId, result, cancellationToken, "LeadStatus");
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Fatal error in inbound lead status sync for dealership {DealershipId}", dealershipId);
            var failResult = SyncResult.Failure(ex.Message, sw.ElapsedMilliseconds);
            await TryCreateSyncLogAsync(dealershipId, failResult, cancellationToken, "LeadStatus");
            return failResult;
        }
    }

    private async Task ApplyConflictStrategyAsync(
        Lead lead,
        LeadStatus remoteLeadStatus,
        int remoteEasyCarsStatus,
        CancellationToken cancellationToken)
    {
        switch (_conflictStrategy)
        {
            case ConflictResolutionStrategy.LocalWins:
                _logger.LogDebug("Conflict for lead {LeadId}: LocalWins — keeping local status {Status}", lead.Id, lead.Status);
                break;

            case ConflictResolutionStrategy.RemoteWins:
                if (!lead.CanChangeStatusTo(remoteLeadStatus))
                {
                    _logger.LogWarning("Conflict for lead {LeadId}: RemoteWins but cannot un-delete. Creating ManualReview conflict.", lead.Id);
                    await CreateConflictRecordAsync(lead, remoteEasyCarsStatus, cancellationToken);
                    return;
                }
                lead.UpdateStatus(remoteLeadStatus);
                lead.MarkStatusSyncedToEasyCars(remoteEasyCarsStatus);
                _logger.LogInformation("Lead {LeadId} status updated to {Status} (RemoteWins)", lead.Id, remoteLeadStatus);
                break;

            case ConflictResolutionStrategy.ManualReview:
                var alreadyExists = await _conflictRepository.ExistsUnresolvedForLeadAsync(lead.Id, cancellationToken);
                if (!alreadyExists)
                    await CreateConflictRecordAsync(lead, remoteEasyCarsStatus, cancellationToken);
                break;
        }
    }

    private async Task CreateConflictRecordAsync(Lead lead, int remoteEasyCarsStatus, CancellationToken cancellationToken)
    {
        var conflict = LeadStatusConflict.Create(
            lead.DealershipId,
            lead.Id,
            lead.EasyCarsLeadNumber!,
            lead.Status.ToString(),
            remoteEasyCarsStatus);
        await _conflictRepository.AddAsync(conflict, cancellationToken);
        _logger.LogInformation("Created LeadStatusConflict for lead {LeadId}: local={Local}, remote={Remote}",
            lead.Id, lead.Status, remoteEasyCarsStatus);
    }

    private async Task CreateSyncLogAsync(int dealershipId, SyncResult result, CancellationToken cancellationToken, string syncType = "Lead")
    {
        var syncLog = EasyCarsSyncLog.Create(
            dealershipId: dealershipId,
            status: result.Status,
            itemsProcessed: result.ItemsProcessed,
            itemsSucceeded: result.ItemsSucceeded,
            itemsFailed: result.ItemsFailed,
            errors: result.Errors,
            durationMs: result.DurationMs,
            syncType: syncType
        );
        await _syncLogRepository.AddAsync(syncLog, cancellationToken);
        _logger.LogInformation("Created sync log for dealership {DealershipId}", dealershipId);
    }

    private async Task TryCreateSyncLogAsync(int dealershipId, SyncResult result, CancellationToken cancellationToken, string syncType = "Lead")
    {
        try
        {
            await CreateSyncLogAsync(dealershipId, result, cancellationToken, syncType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create sync log for dealership {DealershipId}. Sync operation completed, but audit trail may be incomplete.", dealershipId);
        }
    }
}
