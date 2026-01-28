using MediatR;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.PromotionalPanel.Commands;

public record DeletePromotionalPanelCommand(int Id) : IRequest<bool>;

public class DeletePromotionalPanelCommandHandler : IRequestHandler<DeletePromotionalPanelCommand, bool>
{
    private readonly IPromotionalPanelRepository _repository;

    public DeletePromotionalPanelCommandHandler(IPromotionalPanelRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeletePromotionalPanelCommand request, CancellationToken cancellationToken)
    {
        var panel = await _repository.GetByIdAsync(request.Id);
        if (panel == null)
            return false;

        _repository.Delete(panel);
        await _repository.SaveChangesAsync();
        return true;
    }
}
