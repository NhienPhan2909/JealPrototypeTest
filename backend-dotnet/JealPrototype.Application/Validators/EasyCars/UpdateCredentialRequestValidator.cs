using FluentValidation;
using JealPrototype.Application.DTOs.EasyCars;

namespace JealPrototype.Application.Validators.EasyCars;

/// <summary>
/// Validator for UpdateCredentialRequest
/// </summary>
public class UpdateCredentialRequestValidator : AbstractValidator<UpdateCredentialRequest>
{
    public UpdateCredentialRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.ClientId))
            .WithMessage("Client ID cannot exceed 200 characters");

        RuleFor(x => x.ClientSecret)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.ClientSecret))
            .WithMessage("Client Secret cannot exceed 200 characters");

        RuleFor(x => x.AccountNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.AccountNumber))
            .WithMessage("Account Number cannot exceed 100 characters");

        RuleFor(x => x.AccountSecret)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.AccountSecret))
            .WithMessage("Account Secret cannot exceed 100 characters");

        RuleFor(x => x.Environment)
            .Must(env => env == "Test" || env == "Production")
            .When(x => !string.IsNullOrEmpty(x.Environment))
            .WithMessage("Environment must be 'Test' or 'Production'");

        RuleFor(x => x.YardCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.YardCode))
            .WithMessage("Yard Code cannot exceed 50 characters");
    }
}
