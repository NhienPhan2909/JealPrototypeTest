using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class PromotionalPanelRepository : IPromotionalPanelRepository
{
    private readonly ApplicationDbContext _context;

    public PromotionalPanelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PromotionalPanel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.PromotionalPanels.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<PromotionalPanel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PromotionalPanels.ToListAsync(cancellationToken);
    }

    public async Task<PromotionalPanel> AddAsync(PromotionalPanel entity, CancellationToken cancellationToken = default)
    {
        await _context.PromotionalPanels.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(PromotionalPanel entity, CancellationToken cancellationToken = default)
    {
        _context.PromotionalPanels.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(PromotionalPanel entity, CancellationToken cancellationToken = default)
    {
        _context.PromotionalPanels.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(PromotionalPanel entity)
    {
        _context.PromotionalPanels.Update(entity);
    }

    public void Delete(PromotionalPanel entity)
    {
        _context.PromotionalPanels.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<PromotionalPanel>> GetByDealershipIdAsync(int dealershipId)
    {
        return await _context.PromotionalPanels
            .Where(p => p.DealershipId == dealershipId && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
    }
}
