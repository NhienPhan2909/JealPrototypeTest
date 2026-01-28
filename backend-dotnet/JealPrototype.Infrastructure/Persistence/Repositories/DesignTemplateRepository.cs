using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class DesignTemplateRepository : IDesignTemplateRepository
{
    private readonly ApplicationDbContext _context;

    public DesignTemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DesignTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.DesignTemplates.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<DesignTemplate>> GetPresetsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DesignTemplates
            .Where(dt => dt.IsPreset)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DesignTemplate>> GetByDealershipAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.DesignTemplates
            .Where(dt => dt.DealershipId == dealershipId || dt.IsPreset)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DesignTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DesignTemplates.ToListAsync(cancellationToken);
    }

    public async Task<DesignTemplate> AddAsync(DesignTemplate entity, CancellationToken cancellationToken = default)
    {
        _context.DesignTemplates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(DesignTemplate entity, CancellationToken cancellationToken = default)
    {
        _context.DesignTemplates.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DesignTemplate entity, CancellationToken cancellationToken = default)
    {
        _context.DesignTemplates.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(DesignTemplate entity)
    {
        _context.DesignTemplates.Update(entity);
    }

    public void Delete(DesignTemplate entity)
    {
        _context.DesignTemplates.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
