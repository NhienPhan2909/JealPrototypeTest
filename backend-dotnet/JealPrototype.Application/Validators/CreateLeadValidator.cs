using FluentValidation;
using JealPrototype.Application.DTOs.Lead;

namespace JealPrototype.Application.Validators;

public class CreateLeadValidator : AbstractValidator<CreateLeadDto>
{
    public CreateLeadValidator()
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

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(5000).WithMessage("Message must not exceed 5000 characters");
    }
}
