using JealPrototype.Application.DTOs.DesignTemplate;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.DesignTemplates;

public class GetDesignTemplatesUseCase
{
    private readonly IDesignTemplateRepository _repository;

    public GetDesignTemplatesUseCase(IDesignTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<DesignTemplateResponseDto>> ExecuteAsync(int? dealershipId = null)
    {
        var templates = dealershipId.HasValue
            ? await _repository.GetByDealershipAsync(dealershipId.Value)
            : await _repository.GetPresetsAsync();

        return templates.Select(t => new DesignTemplateResponseDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            DealershipId = t.DealershipId,
            IsPreset = t.IsPreset,
            ThemeColor = t.ThemeColor.Value,
            SecondaryThemeColor = t.SecondaryThemeColor.Value,
            BodyBackgroundColor = t.BodyBackgroundColor.Value,
            FontFamily = t.FontFamily,
            CreatedAt = t.CreatedAt
        });
    }
}
