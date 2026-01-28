using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface IBlogPostRepository : IRepository<BlogPost>
{
    Task<IEnumerable<BlogPost>> GetByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlogPost>> GetPublishedByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default);
    Task<BlogPost?> GetBySlugAsync(string slug, int dealershipId, CancellationToken cancellationToken = default);
    Task<BlogPost?> GetByIdAndDealershipAsync(int id, int dealershipId, CancellationToken cancellationToken = default);
}
