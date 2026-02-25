using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for EasyCars credentials
/// </summary>
public class EasyCarsCredentialRepository : IEasyCarsCredentialRepository
{
    private readonly ApplicationDbContext _context;

    public EasyCarsCredentialRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<EasyCarsCredential> CreateAsync(EasyCarsCredential credential)
    {
        _context.EasyCarsCredentials.Add(credential);
        await _context.SaveChangesAsync();
        return credential;
    }

    public async Task<EasyCarsCredential?> GetByDealershipIdAsync(int dealershipId)
    {
        return await _context.EasyCarsCredentials
            .FirstOrDefaultAsync(c => c.DealershipId == dealershipId);
    }

    public async Task<EasyCarsCredential?> GetByIdAsync(int id)
    {
        return await _context.EasyCarsCredentials
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<EasyCarsCredential> UpdateAsync(EasyCarsCredential credential)
    {
        _context.EasyCarsCredentials.Update(credential);
        await _context.SaveChangesAsync();
        return credential;
    }

    public async Task DeleteAsync(int id)
    {
        var credential = await GetByIdAsync(id);
        if (credential != null)
        {
            _context.EasyCarsCredentials.Remove(credential);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsForDealershipAsync(int dealershipId)
    {
        return await _context.EasyCarsCredentials
            .AnyAsync(c => c.DealershipId == dealershipId);
    }
}
