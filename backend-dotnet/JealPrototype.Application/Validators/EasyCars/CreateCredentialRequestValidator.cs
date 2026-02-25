using FluentValidation;
using JealPrototype.Application.DTOs.EasyCars;

namespace JealPrototype.Application.Validators.EasyCars;

/// <summary>
/// Validator for CreateCredentialRequest
/// </summary>
public class CreateCredentialRequestValidator : AbstractValidator<CreateCredentialRequest>
{
    public CreateCredentialRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required")
            .MaximumLength(200)
            .WithMessage("Client ID cannot exceed 200 characters");

        RuleFor(x => x.ClientSecret)
            .NotEmpty()
            .WithMessage("Client Secret is required")
            .MaximumLength(200)
            .WithMessage("Client Secret cannot exceed 200 characters");

        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("Account Number is required")
            .MaximumLength(100)
            .WithMessage("Account Number cannot exceed 100 characters");

        RuleFor(x => x.AccountSecret)
            .NotEmpty()
            .WithMessage("Account Secret is required")
            .MaximumLength(100)
            .WithMessage("Account Secret cannot exceed 100 characters");

        RuleFor(x => x.Environment)
            .NotEmpty()
            .WithMessage("Environment is required")
            .Must(env => env == "Test" || env == "Production")
            .WithMessage("Environment must be 'Test' or 'Production'");

        RuleFor(x => x.YardCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.YardCode))
            .WithMessage("Yard Code cannot exceed 50 characters");
    }
}
