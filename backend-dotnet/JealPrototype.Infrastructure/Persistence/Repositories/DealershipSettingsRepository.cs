using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class DealershipSettingsRepository : IDealershipSettingsRepository
{
    private readonly ApplicationDbContext _context;
    
    public DealershipSettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<DealershipSettings?> GetByDealershipIdAsync(
        int dealershipId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.DealershipSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(ds => ds.DealershipId == dealershipId, cancellationToken);
    }
    
    public async Task<List<int>> GetDealershipsWithAutoSyncEnabledAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.DealershipSettings
            .AsNoTracking()
            .Where(ds => ds.EasyCarAutoSyncEnabled)
            .Select(ds => ds.DealershipId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<DealershipSettings> AddAsync(
        DealershipSettings settings, 
        CancellationToken cancellationToken = default)
    {
        _context.DealershipSettings.Add(settings);
        await _context.SaveChangesAsync(cancellationToken);
        return settings;
    }
    
    public async Task UpdateAsync(
        DealershipSettings settings, 
        CancellationToken cancellationToken = default)
    {
        _context.DealershipSettings.Update(settings);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
