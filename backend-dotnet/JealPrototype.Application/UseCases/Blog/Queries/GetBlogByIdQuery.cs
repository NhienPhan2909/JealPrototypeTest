using MediatR;
using JealPrototype.Application.DTOs.Blog;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Blog.Queries;

public record GetBlogByIdQuery(int Id) : IRequest<BlogDto?>;

public class GetBlogByIdQueryHandler : IRequestHandler<GetBlogByIdQuery, BlogDto?>
{
    private readonly IBlogRepository _blogRepository;

    public GetBlogByIdQueryHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<BlogDto?> Handle(GetBlogByIdQuery request, CancellationToken cancellationToken)
    {
        var blog = await _blogRepository.GetByIdAsync(request.Id);
        if (blog == null)
            return null;

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
