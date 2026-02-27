using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for EasyCars sync log operations
/// </summary>
public class EasyCarsSyncLogRepository : IEasyCarsSyncLogRepository
{
    private readonly ApplicationDbContext _context;

    public EasyCarsSyncLogRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<EasyCarsSyncLog> AddAsync(EasyCarsSyncLog syncLog, CancellationToken cancellationToken = default)
    {
        if (syncLog == null)
            throw new ArgumentNullException(nameof(syncLog));

        await _context.EasyCarsSyncLogs.AddAsync(syncLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return syncLog;
    }

    public async Task<EasyCarsSyncLog?> GetLastSyncAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.EasyCarsSyncLogs
            .Where(l => l.DealershipId == dealershipId)
            .OrderByDescending(l => l.SyncedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<EasyCarsSyncLog>> GetSyncHistoryAsync(int dealershipId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.EasyCarsSyncLogs
            .Where(l => l.DealershipId == dealershipId)
            .OrderByDescending(l => l.SyncedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<EasyCarsSyncLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.EasyCarsSyncLogs
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<(List<EasyCarsSyncLog> Logs, int Total)> GetPagedHistoryAsync(int dealershipId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var query = _context.EasyCarsSyncLogs
            .Where(l => l.DealershipId == dealershipId)
            .OrderByDescending(l => l.SyncedAt);

        var total = await query.CountAsync(cancellationToken);

        var logs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (logs, total);
    }

    public async Task<EasyCarsSyncLog?> GetLastSyncByTypeAsync(int dealershipId, string syncType, CancellationToken cancellationToken = default)
    {
        return await _context.EasyCarsSyncLogs
            .Where(l => l.DealershipId == dealershipId && l.SyncType == syncType)
            .OrderByDescending(l => l.SyncedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(List<EasyCarsSyncLog> Logs, int Total)> GetPagedHistoryByTypeAsync(int dealershipId, string syncType, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var query = _context.EasyCarsSyncLogs
            .Where(l => l.DealershipId == dealershipId && l.SyncType == syncType)
            .OrderByDescending(l => l.SyncedAt);

        var total = await query.CountAsync(cancellationToken);

        var logs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (logs, total);
    }
}
