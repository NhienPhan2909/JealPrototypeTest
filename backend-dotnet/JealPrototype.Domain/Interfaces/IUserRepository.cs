using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default);
}
