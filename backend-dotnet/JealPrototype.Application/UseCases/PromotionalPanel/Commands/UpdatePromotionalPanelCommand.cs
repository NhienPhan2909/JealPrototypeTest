using MediatR;
using JealPrototype.Application.DTOs.PromotionalPanel;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.PromotionalPanel.Commands;

public record UpdatePromotionalPanelCommand(int Id, UpdatePromotionalPanelDto Dto) : IRequest<PromotionalPanelDto?>;

public class UpdatePromotionalPanelCommandHandler : IRequestHandler<UpdatePromotionalPanelCommand, PromotionalPanelDto?>
{
    private readonly IPromotionalPanelRepository _repository;

    public UpdatePromotionalPanelCommandHandler(IPromotionalPanelRepository repository)
    {
        _repository = repository;
    }

    public async Task<PromotionalPanelDto?> Handle(UpdatePromotionalPanelCommand request, CancellationToken cancellationToken)
    {
        var panel = await _repository.GetByIdAsync(request.Id);
        if (panel == null)
            return null;

        if (request.Dto.Title != null)
            panel.Title = request.Dto.Title;
        if (request.Dto.Description != null)
            panel.Description = request.Dto.Description;
        if (request.Dto.ImageUrl != null)
            panel.ImageUrl = request.Dto.ImageUrl;
        if (request.Dto.LinkUrl != null)
            panel.LinkUrl = request.Dto.LinkUrl;
        if (request.Dto.DisplayOrder.HasValue)
            panel.DisplayOrder = request.Dto.DisplayOrder.Value;
        if (request.Dto.IsActive.HasValue)
            panel.IsActive = request.Dto.IsActive.Value;

        _repository.Update(panel);
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
