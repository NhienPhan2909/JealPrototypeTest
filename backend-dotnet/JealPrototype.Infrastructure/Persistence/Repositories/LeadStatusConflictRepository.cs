using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class LeadStatusConflictRepository : ILeadStatusConflictRepository
{
    private readonly ApplicationDbContext _context;

    public LeadStatusConflictRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeadStatusConflict> AddAsync(LeadStatusConflict conflict, CancellationToken ct = default)
    {
        await _context.LeadStatusConflicts.AddAsync(conflict, ct);
        await _context.SaveChangesAsync(ct);
        return conflict;
    }

    public async Task<LeadStatusConflict?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.LeadStatusConflicts.FindAsync(new object[] { id }, ct);

    public async Task<List<LeadStatusConflict>> GetUnresolvedByDealershipAsync(int dealershipId, CancellationToken ct = default)
        => await _context.LeadStatusConflicts
            .Where(c => c.DealershipId == dealershipId && !c.IsResolved)
            .OrderByDescending(c => c.DetectedAt)
            .ToListAsync(ct);

    public async Task UpdateAsync(LeadStatusConflict conflict, CancellationToken ct = default)
    {
        _context.LeadStatusConflicts.Update(conflict);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsUnresolvedForLeadAsync(int leadId, CancellationToken ct = default)
        => await _context.LeadStatusConflicts
            .AnyAsync(c => c.LeadId == leadId && !c.IsResolved, ct);
}
