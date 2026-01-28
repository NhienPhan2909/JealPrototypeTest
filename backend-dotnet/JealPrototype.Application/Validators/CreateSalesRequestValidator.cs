using FluentValidation;
using JealPrototype.Application.DTOs.SalesRequest;

namespace JealPrototype.Application.Validators;

public class CreateSalesRequestValidator : AbstractValidator<CreateSalesRequestDto>
{
    public CreateSalesRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters");

        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required")
            .MaximumLength(100).WithMessage("Make must not exceed 100 characters");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(100).WithMessage("Model must not exceed 100 characters");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.Now.Year + 1).WithMessage($"Year must be between 1900 and {DateTime.Now.Year + 1}");

        RuleFor(x => x.Kilometers)
            .GreaterThanOrEqualTo(0).WithMessage("Kilometers must be non-negative");
    }
}
