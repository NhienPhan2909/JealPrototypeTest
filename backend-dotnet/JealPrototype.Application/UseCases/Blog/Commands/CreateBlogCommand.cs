using MediatR;
using JealPrototype.Application.DTOs.Blog;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Domain.Entities;

namespace JealPrototype.Application.UseCases.Blog.Commands;

public record CreateBlogCommand(CreateBlogDto Dto) : IRequest<BlogDto>;

public class CreateBlogCommandHandler : IRequestHandler<CreateBlogCommand, BlogDto>
{
    private readonly IBlogRepository _blogRepository;

    public CreateBlogCommandHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<BlogDto> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        var blog = new Domain.Entities.Blog
        {
            DealershipId = request.Dto.DealershipId,
            Title = request.Dto.Title,
            Content = request.Dto.Content,
            ImageUrl = request.Dto.ImageUrl,
            PublishedDate = request.Dto.PublishedDate,
            Author = request.Dto.Author
        };

        await _blogRepository.AddAsync(blog);
        await _blogRepository.SaveChangesAsync();

        return new BlogDto
        {
            Id = blog.Id,
            DealershipId = blog.DealershipId,
            Title = blog.Title,
            Content = blog.Content,
            ImageUrl = blog.ImageUrl,
            PublishedDate = blog.PublishedDate,
            Author = blog.Author,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt
        };
    }
}
