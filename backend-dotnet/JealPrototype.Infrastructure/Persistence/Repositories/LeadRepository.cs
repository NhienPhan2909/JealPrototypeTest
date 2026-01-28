using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly ApplicationDbContext _context;

    public LeadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Leads.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Lead>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Leads.ToListAsync(cancellationToken);
    }

    public async Task<Lead> AddAsync(Lead entity, CancellationToken cancellationToken = default)
    {
        await _context.Leads.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Lead entity, CancellationToken cancellationToken = default)
    {
        _context.Leads.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Lead entity, CancellationToken cancellationToken = default)
    {
        _context.Leads.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Lead entity)
    {
        _context.Leads.Update(entity);
    }

    public void Delete(Lead entity)
    {
        _context.Leads.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lead>> GetByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Include(l => l.Vehicle)
            .Where(l => l.DealershipId == dealershipId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Lead?> GetByIdAndDealershipAsync(int id, int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Include(l => l.Vehicle)
            .FirstOrDefaultAsync(l => l.Id == id && l.DealershipId == dealershipId, cancellationToken);
    }
}
