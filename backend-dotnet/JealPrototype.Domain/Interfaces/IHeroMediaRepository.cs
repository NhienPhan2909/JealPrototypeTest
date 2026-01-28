using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface IHeroMediaRepository : IRepository<HeroMedia>
{
    Task<HeroMedia?> GetByDealershipIdAsync(int dealershipId);
}
