using JealPrototype.Application.DTOs.DesignTemplate;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.DesignTemplates;

public class CreateDesignTemplateUseCase
{
    private readonly IDesignTemplateRepository _repository;

    public CreateDesignTemplateUseCase(IDesignTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<DesignTemplateResponseDto> ExecuteAsync(CreateDesignTemplateDto dto)
    {
        var template = DesignTemplate.Create(
            dto.Name,
            dto.ThemeColor,
            dto.SecondaryThemeColor,
            dto.BodyBackgroundColor,
            dto.FontFamily,
            dto.DealershipId,
            dto.Description,
            dto.IsPreset
        );

        var created = await _repository.AddAsync(template);

        return new DesignTemplateResponseDto
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description,
            DealershipId = created.DealershipId,
            IsPreset = created.IsPreset,
            ThemeColor = created.ThemeColor.Value,
            SecondaryThemeColor = created.SecondaryThemeColor.Value,
            BodyBackgroundColor = created.BodyBackgroundColor.Value,
            FontFamily = created.FontFamily,
            CreatedAt = created.CreatedAt
        };
    }
}
