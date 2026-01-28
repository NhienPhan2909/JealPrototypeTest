using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class SalesRequestRepository : ISalesRequestRepository
{
    private readonly ApplicationDbContext _context;

    public SalesRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalesRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.SalesRequests.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<SalesRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SalesRequests.ToListAsync(cancellationToken);
    }

    public async Task<SalesRequest> AddAsync(SalesRequest entity, CancellationToken cancellationToken = default)
    {
        await _context.SalesRequests.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(SalesRequest entity, CancellationToken cancellationToken = default)
    {
        _context.SalesRequests.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(SalesRequest entity, CancellationToken cancellationToken = default)
    {
        _context.SalesRequests.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(SalesRequest entity)
    {
        _context.SalesRequests.Update(entity);
    }

    public void Delete(SalesRequest entity)
    {
        _context.SalesRequests.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<SalesRequest>> GetByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.SalesRequests
            .Where(sr => sr.DealershipId == dealershipId)
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SalesRequest?> GetByIdAndDealershipAsync(int id, int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.SalesRequests
            .FirstOrDefaultAsync(sr => sr.Id == id && sr.DealershipId == dealershipId, cancellationToken);
    }
}
