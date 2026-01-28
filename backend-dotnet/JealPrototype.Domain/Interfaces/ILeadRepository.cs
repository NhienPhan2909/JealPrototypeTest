using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface ILeadRepository : IRepository<Lead>
{
    Task<IEnumerable<Lead>> GetByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default);
    Task<Lead?> GetByIdAndDealershipAsync(int id, int dealershipId, CancellationToken cancellationToken = default);
}
