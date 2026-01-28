using FluentValidation;
using JealPrototype.Application.DTOs.Vehicle;

namespace JealPrototype.Application.Validators;

public class CreateVehicleValidator : AbstractValidator<CreateVehicleDto>
{
    public CreateVehicleValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required")
            .MaximumLength(100).WithMessage("Make must not exceed 100 characters");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(100).WithMessage("Model must not exceed 100 characters");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, 2100).WithMessage("Year must be between 1900 and 2100");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage must be non-negative");

        RuleFor(x => x.Condition)
            .NotEmpty().WithMessage("Condition is required")
            .Must(c => c == "new" || c == "used").WithMessage("Condition must be 'new' or 'used'");

        RuleFor(x => x.Status)
            .Must(s => s == "draft" || s == "active" || s == "pending" || s == "sold")
            .WithMessage("Status must be one of: draft, active, pending, sold");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
