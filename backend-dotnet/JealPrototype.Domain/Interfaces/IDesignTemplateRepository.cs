using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

public interface IDesignTemplateRepository : IRepository<DesignTemplate>
{
    Task<IEnumerable<DesignTemplate>> GetPresetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DesignTemplate>> GetByDealershipAsync(int dealershipId, CancellationToken cancellationToken = default);
}
