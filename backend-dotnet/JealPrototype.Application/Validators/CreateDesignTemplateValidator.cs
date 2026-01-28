using FluentValidation;
using JealPrototype.Application.DTOs.DesignTemplate;

namespace JealPrototype.Application.Validators;

public class CreateDesignTemplateValidator : AbstractValidator<CreateDesignTemplateDto>
{
    public CreateDesignTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.ThemeColor)
            .NotEmpty().WithMessage("Theme color is required")
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Theme color must be a valid hex color (e.g., #3B82F6)");

        RuleFor(x => x.SecondaryThemeColor)
            .NotEmpty().WithMessage("Secondary theme color is required")
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Secondary theme color must be a valid hex color");

        RuleFor(x => x.BodyBackgroundColor)
            .NotEmpty().WithMessage("Body background color is required")
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Body background color must be a valid hex color");

        RuleFor(x => x.FontFamily)
            .NotEmpty().WithMessage("Font family is required")
            .MaximumLength(100).WithMessage("Font family must not exceed 100 characters");

        // Validate preset/dealership constraint
        RuleFor(x => x)
            .Must(dto => (dto.IsPreset && dto.DealershipId == null) || (!dto.IsPreset && dto.DealershipId != null))
            .WithMessage("Preset templates must have no dealership, custom templates must have a dealership");
    }
}
