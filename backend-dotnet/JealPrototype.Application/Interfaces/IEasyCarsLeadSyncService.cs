using JealPrototype.Application.DTOs.EasyCars;

namespace JealPrototype.Application.Interfaces;

/// <summary>
/// Service for orchestrating outbound lead synchronization from local system to EasyCars.
/// Inbound synchronization (EasyCars to local) is implemented in Story 3.4.
/// </summary>
public interface IEasyCarsLeadSyncService
{
    /// <summary>
    /// Synchronizes a local lead to EasyCars API.
    /// Creates new lead if EasyCarsLeadNumber is null; updates existing lead otherwise.
    /// </summary>
    Task<SyncResult> SyncLeadToEasyCarsAsync(int leadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches updated lead data from EasyCars for all known leads of the dealership.
    /// Updates local lead records with latest EasyCars metadata.
    /// </summary>
    Task<SyncResult> SyncLeadsFromEasyCarsAsync(int dealershipId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pushes the current local status of a single lead to EasyCars via UpdateLead.
    /// Only executes if the lead has an EasyCarsLeadNumber. Status-only update.
    /// </summary>
    Task<SyncResult> SyncLeadStatusToEasyCarsAsync(int leadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inbound: checks EasyCars for status changes on all known leads of the dealership.
    /// Applies the configured ConflictResolutionStrategy.
    /// </summary>
    Task<SyncResult> SyncLeadStatusesFromEasyCarsAsync(int dealershipId, CancellationToken cancellationToken = default);
}
