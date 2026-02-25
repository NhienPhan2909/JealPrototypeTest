using Hangfire;
using JealPrototype.Application.BackgroundJobs;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace JealPrototype.API.Controllers;

/// <summary>
/// Controller for EasyCars stock synchronization admin interface (Story 2.5)
/// Provides endpoints for manual sync trigger, status, and history
/// </summary>
[ApiController]
[Route("api/easycars")]
[Authorize]
public class EasyCarsStockSyncController : ControllerBase
{
    private readonly IEasyCarsSyncLogRepository _syncLogRepository;
    private readonly IEasyCarsCredentialRepository _credentialRepository;
    private readonly ILogger<EasyCarsStockSyncController> _logger;

    public EasyCarsStockSyncController(
        IEasyCarsSyncLogRepository syncLogRepository,
        IEasyCarsCredentialRepository credentialRepository,
        ILogger<EasyCarsStockSyncController> logger)
    {
        _syncLogRepository = syncLogRepository ?? throw new ArgumentNullException(nameof(syncLogRepository));
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets current sync status for the authenticated dealership
    /// Endpoint: GET /api/easycars/sync-status
    /// </summary>
    [HttpGet("sync-status")]
    [ProducesResponseType(typeof(SyncStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SyncStatusDto>> GetSyncStatus(CancellationToken cancellationToken)
    {
        try
        {
            var dealershipId = GetDealershipIdFromClaims();
            _logger.LogInformation("Fetching sync status for dealership {DealershipId}", dealershipId);

            // Get last sync log
            var lastSync = await _syncLogRepository.GetLastSyncAsync(dealershipId, cancellationToken);

            // Check if credentials exist
            var credential = await _credentialRepository.GetByDealershipIdAsync(dealershipId);
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
            _logger.LogWarning(ex, "Unauthorized sync status access");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sync status");
            return StatusCode(500, new { error = "An error occurred while fetching sync status" });
        }
    }

    /// <summary>
    /// Triggers manual stock synchronization for the authenticated dealership
    /// Endpoint: POST /api/easycars/sync/trigger
    /// </summary>
    [HttpPost("sync/trigger")]
    [Authorize(Roles = "Admin,DealershipAdmin")]
    [ProducesResponseType(typeof(TriggerSyncResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TriggerSyncResponse>> TriggerSync(CancellationToken cancellationToken)
    {
        try
        {
            var dealershipId = GetDealershipIdFromClaims();
            _logger.LogInformation("Manual sync triggered for dealership {DealershipId}", dealershipId);

            // Validate credentials exist
            var credential = await _credentialRepository.GetByDealershipIdAsync(dealershipId);
            if (credential == null)
            {
                _logger.LogWarning("Sync trigger failed: No credentials for dealership {DealershipId}", dealershipId);
                return BadRequest(new { error = "EasyCars credentials not configured. Please configure credentials first." });
            }

            // Rate limiting: prevent sync spam (max 1 manual trigger per 60 seconds)
            var lastSync = await _syncLogRepository.GetLastSyncAsync(dealershipId, cancellationToken);
            if (lastSync != null && (DateTime.UtcNow - lastSync.SyncedAt).TotalSeconds < 60)
            {
                _logger.LogWarning("Rate limit exceeded: Sync already triggered for dealership {DealershipId} at {LastSync}", 
                    dealershipId, lastSync.SyncedAt);
                return StatusCode(StatusCodes.Status429TooManyRequests, 
                    new { error = "Sync already triggered recently. Please wait 60 seconds before triggering again." });
            }

            // Enqueue background job using Hangfire
            var jobId = BackgroundJob.Enqueue<StockSyncBackgroundJob>(job => 
                job.ExecuteManualSyncAsync(dealershipId, CancellationToken.None));

            _logger.LogInformation("Sync job {JobId} enqueued for dealership {DealershipId}", jobId, dealershipId);

            var response = new TriggerSyncResponse
            {
                Message = "Sync started successfully",
                JobId = jobId
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized sync trigger attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering sync");
            return StatusCode(500, new { error = "An error occurred while triggering sync" });
        }
    }

    /// <summary>
    /// Gets paginated sync history for the authenticated dealership
    /// Endpoint: GET /api/easycars/sync-history?page=1&pageSize=10
    /// </summary>
    [HttpGet("sync-history")]
    [ProducesResponseType(typeof(SyncHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SyncHistoryResponse>> GetSyncHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dealershipId = GetDealershipIdFromClaims();
            _logger.LogInformation("Fetching sync history for dealership {DealershipId} (page {Page}, size {PageSize})", 
                dealershipId, page, pageSize);

            var (logs, total) = await _syncLogRepository.GetPagedHistoryAsync(dealershipId, page, pageSize, cancellationToken);

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
            _logger.LogWarning(ex, "Unauthorized sync history access");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sync history");
            return StatusCode(500, new { error = "An error occurred while fetching sync history" });
        }
    }

    /// <summary>
    /// Gets detailed sync log by ID for the authenticated dealership
    /// Endpoint: GET /api/easycars/sync-logs/:id
    /// </summary>
    [HttpGet("sync-logs/{id}")]
    [ProducesResponseType(typeof(SyncLogDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SyncLogDetailsDto>> GetSyncLogDetails(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var dealershipId = GetDealershipIdFromClaims();
            _logger.LogInformation("Fetching sync log {LogId} for dealership {DealershipId}", id, dealershipId);

            var log = await _syncLogRepository.GetByIdAsync(id, cancellationToken);

            if (log == null)
            {
                _logger.LogWarning("Sync log {LogId} not found", id);
                return NotFound(new { error = "Sync log not found" });
            }

            // Verify log belongs to user's dealership (security check)
            if (log.DealershipId != dealershipId)
            {
                _logger.LogWarning("Unauthorized access to sync log {LogId} by dealership {DealershipId}", id, dealershipId);
                return NotFound(new { error = "Sync log not found" });
            }

            // Deserialize error messages
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
            _logger.LogWarning(ex, "Unauthorized sync log access");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sync log details");
            return StatusCode(500, new { error = "An error occurred while fetching sync log details" });
        }
    }

    /// <summary>
    /// Extracts dealership ID from JWT claims
    /// </summary>
    private int GetDealershipIdFromClaims()
    {
        var dealershipIdClaim = User.FindFirst("DealershipId")?.Value;

        if (string.IsNullOrEmpty(dealershipIdClaim) || !int.TryParse(dealershipIdClaim, out int dealershipId))
        {
            throw new UnauthorizedAccessException("Invalid or missing dealership ID in token");
        }

        return dealershipId;
    }
}
