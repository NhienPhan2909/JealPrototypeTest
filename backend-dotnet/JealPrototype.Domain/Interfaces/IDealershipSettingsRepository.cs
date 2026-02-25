using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface IDealershipSettingsRepository
{
    Task<DealershipSettings?> GetByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default);
    Task<List<int>> GetDealershipsWithAutoSyncEnabledAsync(CancellationToken cancellationToken = default);
    Task<DealershipSettings> AddAsync(DealershipSettings settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(DealershipSettings settings, CancellationToken cancellationToken = default);
}
