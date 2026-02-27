using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface ILeadStatusConflictRepository
{
    Task<LeadStatusConflict> AddAsync(LeadStatusConflict conflict, CancellationToken ct = default);
    Task<LeadStatusConflict?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<LeadStatusConflict>> GetUnresolvedByDealershipAsync(int dealershipId, CancellationToken ct = default);
    Task UpdateAsync(LeadStatusConflict conflict, CancellationToken ct = default);
    /// <summary>Returns true if an unresolved conflict already exists for this lead.</summary>
    Task<bool> ExistsUnresolvedForLeadAsync(int leadId, CancellationToken ct = default);
}
