using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface ISystemSettingsRepository
{
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> GetBoolValueAsync(string key, bool defaultValue, CancellationToken cancellationToken = default);
    Task<int> GetIntValueAsync(string key, int defaultValue, CancellationToken cancellationToken = default);
    Task SetValueAsync(string key, string value, CancellationToken cancellationToken = default);
    Task<SystemSettings?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
}
