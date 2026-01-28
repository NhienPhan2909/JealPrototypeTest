using FluentValidation;
using JealPrototype.Application.DTOs.Dealership;

namespace JealPrototype.Application.Validators;

public class CreateDealershipValidator : AbstractValidator<CreateDealershipDto>
{
    public CreateDealershipValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(255).WithMessage("Website URL must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl));
    }
}
