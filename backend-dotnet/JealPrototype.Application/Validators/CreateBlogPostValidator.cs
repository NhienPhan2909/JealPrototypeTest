using FluentValidation;
using JealPrototype.Application.DTOs.BlogPost;

namespace JealPrototype.Application.Validators;

public class CreateBlogPostValidator : AbstractValidator<CreateBlogPostDto>
{
    public CreateBlogPostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");

        RuleFor(x => x.Slug)
            .MaximumLength(255).WithMessage("Slug must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));

        RuleFor(x => x.AuthorName)
            .NotEmpty().WithMessage("Author name is required")
            .MaximumLength(255).WithMessage("Author name must not exceed 255 characters");

        RuleFor(x => x.Status)
            .Must(s => s == "draft" || s == "published" || s == "archived")
            .WithMessage("Status must be one of: draft, published, archived");
    }
}
