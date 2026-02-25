using FluentValidation;
using JealPrototype.Application.DTOs.EasyCars;

namespace JealPrototype.Application.Validators.EasyCars;

/// <summary>
/// Validator for TestConnectionRequest
/// </summary>
public class TestConnectionRequestValidator : AbstractValidator<TestConnectionRequest>
{
    public TestConnectionRequestValidator()
    {
        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("Account Number is required")
            .Matches(@"^[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}$")
            .WithMessage("Account Number must be a valid GUID format");

        RuleFor(x => x.AccountSecret)
            .NotEmpty()
            .WithMessage("Account Secret is required")
            .Matches(@"^[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}$")
            .WithMessage("Account Secret must be a valid GUID format");

        RuleFor(x => x.Environment)
            .NotEmpty()
            .WithMessage("Environment is required")
            .Must(env => env == "Test" || env == "Production")
            .WithMessage("Environment must be 'Test' or 'Production'");
    }
}
