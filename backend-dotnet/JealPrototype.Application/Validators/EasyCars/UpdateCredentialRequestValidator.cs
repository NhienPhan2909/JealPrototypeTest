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
        RuleFor(x => x.AccountNumber)
            .Matches(@"^[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}$")
            .When(x => !string.IsNullOrEmpty(x.AccountNumber))
            .WithMessage("Account Number must be a valid GUID format");

        RuleFor(x => x.AccountSecret)
            .Matches(@"^[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}$")
            .When(x => !string.IsNullOrEmpty(x.AccountSecret))
            .WithMessage("Account Secret must be a valid GUID format");

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
