using Hangfire;
using JealPrototype.Application.BackgroundJobs;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace JealPrototype.API.Controllers;

/// <summary>
/// Controller for EasyCars lead synchronization admin interface (Story 3.5)
/// Provides endpoints for manual lead sync trigger, status, and history
/// </summary>
[ApiController]
[Route("api/easycars")]
[Authorize]
public class EasyCarsLeadSyncController : ControllerBase
{
    private readonly IEasyCarsSyncLogRepository _syncLogRepository;
    private readonly IEasyCarsCredentialRepository _credentialRepository;
    private readonly ILeadStatusConflictRepository _conflictRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly ILogger<EasyCarsLeadSyncController> _logger;

    public EasyCarsLeadSyncController(
        IEasyCarsSyncLogRepository syncLogRepository,
        IEasyCarsCredentialRepository credentialRepository,
        ILeadStatusConflictRepository conflictRepository,
        ILeadRepository leadRepository,
        ILogger<EasyCarsLeadSyncController> logger)
    {
        _syncLogRepository = syncLogRepository ?? throw new ArgumentNullException(nameof(syncLogRepository));
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _conflictRepository = conflictRepository ?? throw new ArgumentNullException(nameof(conflictRepository));
        _leadRepository = leadRepository ?? throw new ArgumentNullException(nameof(leadRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets current lead sync status for the authenticated dealership
    /// Endpoint: GET /api/easycars/lead-sync-status
    /// </summary>
    [HttpGet("lead-sync-status")]
    [ProducesResponseType(typeof(SyncStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SyncStatusDto>> GetLeadSyncStatus([FromQuery] int? dealershipId, CancellationToken cancellationToken)
    {
        try
        {
            var effectiveDealershipId = ResolveEffectiveDealershipId(dealershipId);
            _logger.LogInformation("Fetching lead sync status for dealership {DealershipId}", effectiveDealershipId);

            var lastSync = await _syncLogRepository.GetLastSyncByTypeAsync(effectiveDealershipId, "Lead", cancellationToken);
            var credential = await _credentialRepository.GetByDealershipIdAsync(effectiveDealershipId);
            var hasCredentials = credential != null;

            var response = new SyncStatusDto
            {
                LastSyncedAt = lastSync?.SyncedAt,
                Status = lastSync?.Status,
                ItemsProcessed = lastSync?.ItemsProcessed ?? 0,
                ItemsSucceeded = lastSync?.ItemsSucceeded ?? 0,
                ItemsFailed = lastSync?.ItemsFailed ?? 0,
                DurationMs = lastSync?.DurationMs ?? 0,
                HasCredentials = hasCredentials
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized lead sync status access");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lead sync status");
            return StatusCode(500, new { error = "An error occurred while fetching lead sync status" });
        }
    }

    /// <summary>
    /// Triggers manual lead synchronization for the authenticated dealership
    /// Endpoint: POST /api/easycars/lead-sync/trigger
    /// </summary>
    [HttpPost("lead-sync/trigger")]
    [ProducesResponseType(typeof(TriggerSyncResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TriggerSyncResponse>> TriggerLeadSync([FromQuery] int? dealershipId, CancellationToken cancellationToken)
    {
        try
        {
            var effectiveDealershipId = ResolveEffectiveDealershipId(dealershipId);
            _logger.LogInformation("Manual lead sync triggered for dealership {DealershipId}", effectiveDealershipId);

            var credential = await _credentialRepository.GetByDealershipIdAsync(effectiveDealershipId);
            if (credential == null)
            {
                _logger.LogWarning("Lead sync trigger failed: No credentials for dealership {DealershipId}", effectiveDealershipId);
                return BadRequest(new { error = "EasyCars credentials not configured. Please configure credentials first." });
            }

            // Rate limiting: prevent sync spam (max 1 manual trigger per 60 seconds)
            var lastSync = await _syncLogRepository.GetLastSyncByTypeAsync(effectiveDealershipId, "Lead", cancellationToken);
            if (lastSync != null && (DateTime.UtcNow - lastSync.SyncedAt).TotalSeconds < 60)
            {
                _logger.LogWarning("Rate limit exceeded: Lead sync already triggered for dealership {DealershipId} at {LastSync}",
                    effectiveDealershipId, lastSync.SyncedAt);
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    new { error = "Sync already triggered recently. Please wait 60 seconds before triggering again." });
            }

            var jobId = BackgroundJob.Enqueue<LeadSyncBackgroundJob>(job =>
                job.ExecuteManualLeadSyncAsync(effectiveDealershipId, CancellationToken.None));

            _logger.LogInformation("Lead sync job {JobId} enqueued for dealership {DealershipId}", jobId, effectiveDealershipId);

            var response = new TriggerSyncResponse
            {
                Message = "Lead sync started successfully",
                JobId = jobId
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized lead sync trigger attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering lead sync");
            return StatusCode(500, new { error = "An error occurred while triggering lead sync" });
        }
    }

    /// <summary>
    /// Gets paginated lead sync history for the authenticated dealership
    /// Endpoint: GET /api/easycars/lead-sync-history?page=1&pageSize=10
    /// </summary>
    [HttpGet("lead-sync-history")]
    [ProducesResponseType(typeof(SyncHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SyncHistoryResponse>> GetLeadSyncHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? dealershipId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var effectiveDealershipId = ResolveEffectiveDealershipId(dealershipId);
            _logger.LogInformation("Fetching lead sync history for dealership {DealershipId} (page {Page}, size {PageSize})",
                effectiveDealershipId, page, pageSize);

            var (logs, total) = await _syncLogRepository.GetPagedHistoryByTypeAsync(effectiveDealershipId, "Lead", page, pageSize, cancellationToken);

            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var response = new SyncHistoryResponse
            {
                Logs = logs.Select(log => new SyncHistoryDto
                {
                    Id = log.Id,
                    SyncedAt = log.SyncedAt,
                    Status = log.Status,
                    ItemsProcessed = log.ItemsProcessed,
                    ItemsSucceeded = log.ItemsSucceeded,
                    ItemsFailed = log.ItemsFailed,
                    DurationMs = log.DurationMs
                }).ToList(),
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized lead sync history access");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lead sync history");
            return StatusCode(500, new { error = "An error occurred while fetching lead sync history" });
        }
    }

    /// <summary>
    /// Gets detailed lead sync log by ID for the authenticated dealership
    /// Endpoint: GET /api/easycars/lead-sync-logs/:id
    /// </summary>
    [HttpGet("lead-sync-logs/{id}")]
    [ProducesResponseType(typeof(SyncLogDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SyncLogDetailsDto>> GetLeadSyncLogDetails(
        int id,
        [FromQuery] int? dealershipId,
        CancellationToken cancellationToken)
    {
        try
        {
            var effectiveDealershipId = ResolveEffectiveDealershipId(dealershipId);
            _logger.LogInformation("Fetching lead sync log {LogId} for dealership {DealershipId}", id, effectiveDealershipId);

            var log = await _syncLogRepository.GetByIdAsync(id, cancellationToken);

            if (log == null)
            {
                _logger.LogWarning("Lead sync log {LogId} not found", id);
                return NotFound(new { error = "Sync log not found" });
            }

            if (log.DealershipId != effectiveDealershipId)
            {
                _logger.LogWarning("Unauthorized access to lead sync log {LogId} by dealership {DealershipId}", id, effectiveDealershipId);
                return NotFound(new { error = "Sync log not found" });
            }

            List<string> errors;
            try
            {
                errors = JsonSerializer.Deserialize<List<string>>(log.ErrorMessages) ?? new List<string>();
            }
            catch (JsonException)
            {
                errors = new List<string> { log.ErrorMessages };
            }

            var response = new SyncLogDetailsDto
            {
                Id = log.Id,
                DealershipId = log.DealershipId,
                SyncedAt = log.SyncedAt,
                Status = log.Status,
                ItemsProcessed = log.ItemsProcessed,
                ItemsSucceeded = log.ItemsSucceeded,
                ItemsFailed = log.ItemsFailed,
                Errors = errors,
                DurationMs = log.DurationMs
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized lead sync log access");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lead sync log details");
            return StatusCode(500, new { error = "An error occurred while fetching lead sync log details" });
        }
    }

    /// <summary>
    /// Gets unresolved lead status conflicts for the dealership
    /// Endpoint: GET /api/easycars/lead-sync-conflicts?dealershipId=X
    /// </summary>
    [HttpGet("lead-sync-conflicts")]
    [ProducesResponseType(typeof(List<LeadStatusConflictDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LeadStatusConflictDto>>> GetLeadSyncConflicts(
        [FromQuery] int? dealershipId, CancellationToken cancellationToken)
    {
        try
        {
            var effectiveDealershipId = ResolveEffectiveDealershipId(dealershipId);
            var conflicts = await _conflictRepository.GetUnresolvedByDealershipAsync(effectiveDealershipId, cancellationToken);

            var dtos = conflicts.Select(c => new LeadStatusConflictDto
            {
                Id                 = c.Id,
                LeadId             = c.LeadId,
                EasyCarsLeadNumber = c.EasyCarsLeadNumber,
                LocalStatus        = c.LocalStatus,
                RemoteStatus       = c.RemoteStatus,
                RemoteStatusLabel  = MapEasyCarsStatusLabel(c.RemoteStatus),
                DetectedAt         = c.DetectedAt,
                IsResolved         = c.IsResolved,
                Resolution         = c.Resolution,
                ResolvedAt         = c.ResolvedAt
            }).ToList();

            return Ok(dtos);
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { error = ex.Message }); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lead sync conflicts");
            return StatusCode(500, new { error = "An error occurred while fetching conflicts" });
        }
    }

    /// <summary>
    /// Resolves a lead status conflict
    /// Endpoint: POST /api/easycars/lead-sync-conflicts/{id}/resolve
    /// </summary>
    [HttpPost("lead-sync-conflicts/{id}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveLeadSyncConflict(
        int id, [FromBody] ResolveConflictRequest request,
        [FromQuery] int? dealershipId, CancellationToken cancellationToken)
    {
        try
        {
            var effectiveDealershipId = ResolveEffectiveDealershipId(dealershipId);
            var conflict = await _conflictRepository.GetByIdAsync(id, cancellationToken);

            if (conflict == null || conflict.DealershipId != effectiveDealershipId)
                return NotFound(new { error = "Conflict not found" });

            if (conflict.IsResolved)
                return BadRequest(new { error = "Conflict already resolved" });

            var resolvedBy = User.FindFirst("sub")?.Value ?? User.FindFirst("email")?.Value ?? "admin";
            conflict.Resolve(request.Resolution, resolvedBy);

            if (request.Resolution == "remote")
            {
                var lead = await _leadRepository.GetByIdAsync(conflict.LeadId, cancellationToken);
                if (lead != null && lead.CanChangeStatusTo(MapEasyCarsStatusToLeadStatus(conflict.RemoteStatus)))
                {
                    lead.UpdateStatus(MapEasyCarsStatusToLeadStatus(conflict.RemoteStatus));
                    lead.MarkStatusSyncedToEasyCars(conflict.RemoteStatus);
                    await _leadRepository.UpdateAsync(lead, cancellationToken);
                }
            }

            await _conflictRepository.UpdateAsync(conflict, cancellationToken);
            return Ok(new { message = "Conflict resolved successfully" });
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { error = ex.Message }); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving conflict {ConflictId}", id);
            return StatusCode(500, new { error = "An error occurred while resolving conflict" });
        }
    }

    private static string MapEasyCarsStatusLabel(int status) => status switch
    {
        10 => "New",
        30 => "In Progress",
        50 => "Won",
        60 => "Lost",
        90 => "Deleted",
        _  => $"Unknown ({status})"
    };

    private static LeadStatus MapEasyCarsStatusToLeadStatus(int status) => status switch
    {
        10 => LeadStatus.Received,
        30 => LeadStatus.InProgress,
        50 => LeadStatus.Won,
        60 => LeadStatus.Lost,
        90 => LeadStatus.Deleted,
        _  => LeadStatus.Received
    };

    /// <summary>
    /// Resolves the effective dealership ID from query param (admin) or JWT claims (owner/staff)
    /// </summary>
    private int ResolveEffectiveDealershipId(int? requestedDealershipId)
    {
        var userType = User.FindFirst("usertype")?.Value;

        if ("admin".Equals(userType, StringComparison.OrdinalIgnoreCase))
        {
            if (!requestedDealershipId.HasValue)
                throw new UnauthorizedAccessException("Dealership ID is required for admin users");
            return requestedDealershipId.Value;
        }

        var dealershipIdClaim = User.FindFirst("dealershipid")?.Value;
        if (string.IsNullOrEmpty(dealershipIdClaim) || !int.TryParse(dealershipIdClaim, out int dealershipId))
            throw new UnauthorizedAccessException("Invalid or missing dealership ID in token");

        return dealershipId;
    }
}
