using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class HeroMediaRepository : IHeroMediaRepository
{
    private readonly ApplicationDbContext _context;

    public HeroMediaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HeroMedia?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.HeroMedia.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<HeroMedia>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.HeroMedia.ToListAsync(cancellationToken);
    }

    public async Task<HeroMedia> AddAsync(HeroMedia entity, CancellationToken cancellationToken = default)
    {
        await _context.HeroMedia.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(HeroMedia entity, CancellationToken cancellationToken = default)
    {
        _context.HeroMedia.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(HeroMedia entity, CancellationToken cancellationToken = default)
    {
        _context.HeroMedia.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(HeroMedia entity)
    {
        _context.HeroMedia.Update(entity);
    }

    public void Delete(HeroMedia entity)
    {
        _context.HeroMedia.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<HeroMedia?> GetByDealershipIdAsync(int dealershipId)
    {
        return await _context.HeroMedia
            .FirstOrDefaultAsync(h => h.DealershipId == dealershipId);
    }
}
