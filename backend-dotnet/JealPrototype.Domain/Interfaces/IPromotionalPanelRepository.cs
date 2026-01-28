using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface IPromotionalPanelRepository : IRepository<PromotionalPanel>
{
    Task<List<PromotionalPanel>> GetByDealershipIdAsync(int dealershipId);
}
