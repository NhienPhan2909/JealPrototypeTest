using MediatR;
using JealPrototype.Application.DTOs.PromotionalPanel;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.PromotionalPanel.Queries;

public record GetPromotionalPanelsByDealershipQuery(int DealershipId) : IRequest<List<PromotionalPanelDto>>;

public class GetPromotionalPanelsByDealershipQueryHandler : IRequestHandler<GetPromotionalPanelsByDealershipQuery, List<PromotionalPanelDto>>
{
    private readonly IPromotionalPanelRepository _repository;

    public GetPromotionalPanelsByDealershipQueryHandler(IPromotionalPanelRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PromotionalPanelDto>> Handle(GetPromotionalPanelsByDealershipQuery request, CancellationToken cancellationToken)
    {
        var panels = await _repository.GetByDealershipIdAsync(request.DealershipId);
        return panels.Select(p => new PromotionalPanelDto
        {
            Id = p.Id,
            DealershipId = p.DealershipId,
            Title = p.Title,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            LinkUrl = p.LinkUrl,
            DisplayOrder = p.DisplayOrder,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
    }
}
