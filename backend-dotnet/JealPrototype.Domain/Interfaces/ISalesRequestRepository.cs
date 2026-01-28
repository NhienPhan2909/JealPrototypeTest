using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface ISalesRequestRepository : IRepository<SalesRequest>
{
    Task<IEnumerable<SalesRequest>> GetByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default);
    Task<SalesRequest?> GetByIdAndDealershipAsync(int id, int dealershipId, CancellationToken cancellationToken = default);
}
