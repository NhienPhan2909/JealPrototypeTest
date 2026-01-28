using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface IBlogRepository : IRepository<Blog>
{
    Task<List<Blog>> GetByDealershipIdAsync(int dealershipId);
}
