using MediatR;
using JealPrototype.Application.DTOs.Blog;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Blog.Queries;

public record GetAllBlogsQuery : IRequest<List<BlogDto>>;

public class GetAllBlogsQueryHandler : IRequestHandler<GetAllBlogsQuery, List<BlogDto>>
{
    private readonly IBlogRepository _blogRepository;

    public GetAllBlogsQueryHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<List<BlogDto>> Handle(GetAllBlogsQuery request, CancellationToken cancellationToken)
    {
        var blogs = await _blogRepository.GetAllAsync();
        return blogs.Select(b => new BlogDto
        {
            Id = b.Id,
            DealershipId = b.DealershipId,
            Title = b.Title,
            Content = b.Content,
            ImageUrl = b.ImageUrl,
            PublishedDate = b.PublishedDate,
            Author = b.Author,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        }).ToList();
    }
}
