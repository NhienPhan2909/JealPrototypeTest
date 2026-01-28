using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class DealershipRepository : IDealershipRepository
{
    private readonly ApplicationDbContext _context;

    public DealershipRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dealership?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Dealerships.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Dealership>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Dealerships.ToListAsync(cancellationToken);
    }

    public async Task<Dealership> AddAsync(Dealership entity, CancellationToken cancellationToken = default)
    {
        await _context.Dealerships.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Dealership entity, CancellationToken cancellationToken = default)
    {
        _context.Dealerships.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Dealership entity, CancellationToken cancellationToken = default)
    {
        _context.Dealerships.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Dealership entity)
    {
        _context.Dealerships.Update(entity);
    }

    public void Delete(Dealership entity)
    {
        _context.Dealerships.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dealership?> GetByWebsiteUrlAsync(string websiteUrl, CancellationToken cancellationToken = default)
    {
        return await _context.Dealerships
            .FirstOrDefaultAsync(d => d.WebsiteUrl == websiteUrl, cancellationToken);
    }
}
