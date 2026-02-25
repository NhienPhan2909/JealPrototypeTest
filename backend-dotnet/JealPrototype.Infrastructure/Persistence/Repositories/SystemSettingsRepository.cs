using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class SystemSettingsRepository : ISystemSettingsRepository
{
    private readonly ApplicationDbContext _context;
    
    public SystemSettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<SystemSettings?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }
    
    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await GetByKeyAsync(key, cancellationToken);
        return setting?.Value;
    }
    
    public async Task<bool> GetBoolValueAsync(
        string key, 
        bool defaultValue, 
        CancellationToken cancellationToken = default)
    {
        var value = await GetValueAsync(key, cancellationToken);
        
        if (string.IsNullOrEmpty(value))
            return defaultValue;
        
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }
    
    public async Task<int> GetIntValueAsync(
        string key, 
        int defaultValue, 
        CancellationToken cancellationToken = default)
    {
        var value = await GetValueAsync(key, cancellationToken);
        
        if (string.IsNullOrEmpty(value))
            return defaultValue;
        
        return int.TryParse(value, out var result) ? result : defaultValue;
    }
    
    public async Task SetValueAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var setting = await _context.SystemSettings.FindAsync(new object[] { key }, cancellationToken);
        
        if (setting == null)
        {
            setting = SystemSettings.Create(key, value);
            _context.SystemSettings.Add(setting);
        }
        else
        {
            setting.UpdateValue(value);
            _context.SystemSettings.Update(setting);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
