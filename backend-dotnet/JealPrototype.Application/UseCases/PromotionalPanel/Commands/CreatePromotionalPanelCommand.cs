using MediatR;
using JealPrototype.Application.DTOs.PromotionalPanel;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.PromotionalPanel.Commands;

public record CreatePromotionalPanelCommand(CreatePromotionalPanelDto Dto) : IRequest<PromotionalPanelDto>;

public class CreatePromotionalPanelCommandHandler : IRequestHandler<CreatePromotionalPanelCommand, PromotionalPanelDto>
{
    private readonly IPromotionalPanelRepository _repository;

    public CreatePromotionalPanelCommandHandler(IPromotionalPanelRepository repository)
    {
        _repository = repository;
    }

    public async Task<PromotionalPanelDto> Handle(CreatePromotionalPanelCommand request, CancellationToken cancellationToken)
    {
        var panel = new Domain.Entities.PromotionalPanel
        {
            DealershipId = request.Dto.DealershipId,
            Title = request.Dto.Title,
            Description = request.Dto.Description,
            ImageUrl = request.Dto.ImageUrl,
            LinkUrl = request.Dto.LinkUrl,
            DisplayOrder = request.Dto.DisplayOrder,
            IsActive = request.Dto.IsActive
        };

        await _repository.AddAsync(panel);
        await _repository.SaveChangesAsync();

        return new PromotionalPanelDto
        {
            Id = panel.Id,
            DealershipId = panel.DealershipId,
            Title = panel.Title,
            Description = panel.Description,
            ImageUrl = panel.ImageUrl,
            LinkUrl = panel.LinkUrl,
            DisplayOrder = panel.DisplayOrder,
            IsActive = panel.IsActive,
            CreatedAt = panel.CreatedAt,
            UpdatedAt = panel.UpdatedAt
        };
    }
}
