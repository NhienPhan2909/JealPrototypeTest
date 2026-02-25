using System.Diagnostics;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Interfaces;
using JealPrototype.Application.Interfaces.Security;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.Services.EasyCars;

/// <summary>
/// Service that orchestrates stock synchronization from EasyCars API to our system.
/// Implements Story 2.3: Stock Synchronization Service.
/// Coordinates credential retrieval (Story 1.2), API calls (Story 2.1), 
/// data mapping (Story 2.2), and audit logging (Story 2.3).
/// </summary>
public class EasyCarsStockSyncService : IEasyCarsStockSyncService
{
    private readonly IEasyCarsCredentialRepository _credentialRepository;
    private readonly ICredentialEncryptionService _encryptionService;
    private readonly IEasyCarsApiClient _apiClient;
    private readonly IEasyCarsStockMapper _stockMapper;
    private readonly IEasyCarsSyncLogRepository _syncLogRepository;
    private readonly ILogger<EasyCarsStockSyncService> _logger;

    public EasyCarsStockSyncService(
        IEasyCarsCredentialRepository credentialRepository,
        ICredentialEncryptionService encryptionService,
        IEasyCarsApiClient apiClient,
        IEasyCarsStockMapper stockMapper,
        IEasyCarsSyncLogRepository syncLogRepository,
        ILogger<EasyCarsStockSyncService> logger)
    {
        _credentialRepository = credentialRepository;
        _encryptionService = encryptionService;
        _apiClient = apiClient;
        _stockMapper = stockMapper;
        _syncLogRepository = syncLogRepository;
        _logger = logger;
    }

    public async Task<SyncResult> SyncStockAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting stock synchronization for dealership {DealershipId}", dealershipId);

        try
        {
            // Step 1: Retrieve encrypted credentials
            var credential = await RetrieveCredentialsAsync(dealershipId);

            // Step 2: Fetch stock items from API
            var stockItems = await FetchStockItemsAsync(credential, cancellationToken);

            if (stockItems == null || !stockItems.Any())
            {
                _logger.LogWarning("No stock items found for dealership {DealershipId}", dealershipId);
                stopwatch.Stop();
                var emptyResult = SyncResult.Success(0, stopwatch.ElapsedMilliseconds);
                await CreateSyncLogAsync(dealershipId, emptyResult, cancellationToken);
                return emptyResult;
            }

            // Step 3: Process (map) stock items to vehicles
            var processResult = await ProcessStockItemsAsync(dealershipId, stockItems, stopwatch, cancellationToken);

            // Step 4: Create audit log
            await CreateSyncLogAsync(dealershipId, processResult, cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Stock sync completed for dealership {DealershipId}: Status={Status}, Succeeded={Succeeded}, Failed={Failed}, Duration={DurationMs}ms",
                dealershipId, processResult.Status, processResult.ItemsSucceeded, processResult.ItemsFailed, processResult.DurationMs);

            return processResult;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Stock sync failed for dealership {DealershipId}", dealershipId);

            var failureResult = SyncResult.Failure(ex.Message, stopwatch.ElapsedMilliseconds);

            try
            {
                await CreateSyncLogAsync(dealershipId, failureResult, cancellationToken);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to create sync log for dealership {DealershipId}", dealershipId);
            }

            return failureResult;
        }
    }

    private async Task<EasyCarsCredential> RetrieveCredentialsAsync(int dealershipId)
    {
        _logger.LogInformation("Retrieving EasyCars credentials for dealership {DealershipId}", dealershipId);

        var credential = await _credentialRepository.GetByDealershipIdAsync(dealershipId);
        if (credential == null)
        {
            throw new InvalidOperationException($"No active credentials found for dealership {dealershipId}");
        }

        _logger.LogInformation("Successfully retrieved credentials for dealership {DealershipId}", dealershipId);
        return credential;
    }

    private async Task<List<StockItem>> FetchStockItemsAsync(
        EasyCarsCredential credential,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching stock items from EasyCars API for dealership {DealershipId}",
            credential.DealershipId);

        try
        {
            // Decrypt credentials before passing to API client
            var clientId = await _encryptionService.DecryptAsync(credential.ClientIdEncrypted);
            var clientSecret = await _encryptionService.DecryptAsync(credential.ClientSecretEncrypted);
            var accountNumber = await _encryptionService.DecryptAsync(credential.AccountNumberEncrypted);
            var accountSecret = await _encryptionService.DecryptAsync(credential.AccountSecretEncrypted);

            var stockItems = await _apiClient.GetAdvertisementStocksAsync(
                clientId,
                clientSecret,
                accountNumber,
                accountSecret,
                credential.Environment,
                credential.DealershipId,
                credential.YardCode,
                cancellationToken);

            if (stockItems == null || !stockItems.Any())
            {
                _logger.LogWarning("No stock items returned from EasyCars API for dealership {DealershipId}",
                    credential.DealershipId);
                return new List<StockItem>();
            }

            _logger.LogInformation("Successfully fetched {Count} stock items for dealership {DealershipId}",
                stockItems.Count, credential.DealershipId);

            return stockItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch stock items for dealership {DealershipId}",
                credential.DealershipId);
            throw;
        }
    }

    private async Task<SyncResult> ProcessStockItemsAsync(
        int dealershipId,
        List<StockItem> stockItems,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing {Count} stock items for dealership {DealershipId}",
            stockItems.Count, dealershipId);

        var successCount = 0;
        var failureCount = 0;
        var errorMessages = new List<string>();
        var totalImagesDownloaded = 0;
        var totalImagesFailed = 0;

        foreach (var stockItem in stockItems)
        {
            try
            {
                var mapResult = await _stockMapper.MapToVehicleAsync(stockItem, dealershipId, cancellationToken);
                totalImagesDownloaded += mapResult.ImagesDownloaded;
                totalImagesFailed += mapResult.ImagesFailed;
                successCount++;
            }
            catch (Exception ex)
            {
                failureCount++;
                var errorMsg = $"Failed to map stock item {stockItem.StockNumber}: {ex.Message}";
                errorMessages.Add(errorMsg);
                _logger.LogWarning(ex, errorMsg);
            }
        }

        stopwatch.Stop();

        // Determine sync status based on results
        if (failureCount == 0)
        {
            return SyncResult.Success(successCount, stopwatch.ElapsedMilliseconds, totalImagesDownloaded, totalImagesFailed);
        }
        else if (successCount == 0)
        {
            return SyncResult.Failure(string.Join("; ", errorMessages), stopwatch.ElapsedMilliseconds);
        }
        else
        {
            return SyncResult.PartialSuccess(successCount, failureCount, errorMessages, stopwatch.ElapsedMilliseconds, totalImagesDownloaded, totalImagesFailed);
        }
    }

    private async Task CreateSyncLogAsync(
        int dealershipId,
        SyncResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            var syncLog = EasyCarsSyncLog.Create(
                dealershipId: dealershipId,
                status: result.Status,
                itemsProcessed: result.ItemsProcessed,
                itemsSucceeded: result.ItemsSucceeded,
                itemsFailed: result.ItemsFailed,
                errors: result.Errors,
                durationMs: result.DurationMs
            );

            await _syncLogRepository.AddAsync(syncLog, cancellationToken);

            _logger.LogInformation("Created sync log for dealership {DealershipId}", dealershipId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create sync log for dealership {DealershipId}. Sync operation completed, but audit trail may be incomplete.", dealershipId);
        }
    }
}
