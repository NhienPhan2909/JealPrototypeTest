using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface IDealershipRepository : IRepository<Dealership>
{
    Task<Dealership?> GetByWebsiteUrlAsync(string websiteUrl, CancellationToken cancellationToken = default);
}
