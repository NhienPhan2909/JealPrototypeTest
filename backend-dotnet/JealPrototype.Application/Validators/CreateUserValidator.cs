using FluentValidation;
using JealPrototype.Application.DTOs.User;

namespace JealPrototype.Application.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(255).WithMessage("Full name must not exceed 255 characters");

        RuleFor(x => x.UserType)
            .NotEmpty().WithMessage("User type is required")
            .Must(ut => ut == "admin" || ut == "dealership_owner" || ut == "dealership_staff")
            .WithMessage("User type must be one of: admin, dealership_owner, dealership_staff");

        RuleFor(x => x.DealershipId)
            .NotNull().WithMessage("Dealership ID is required for dealership users")
            .When(x => x.UserType == "dealership_owner" || x.UserType == "dealership_staff");

        RuleFor(x => x.DealershipId)
            .Null().WithMessage("Admin users cannot have a dealership ID")
            .When(x => x.UserType == "admin");
    }
}
