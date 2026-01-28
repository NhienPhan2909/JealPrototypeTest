using MediatR;
using JealPrototype.Application.DTOs.Blog;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Blog.Queries;

public record GetBlogsByDealershipQuery(int DealershipId) : IRequest<List<BlogDto>>;

public class GetBlogsByDealershipQueryHandler : IRequestHandler<GetBlogsByDealershipQuery, List<BlogDto>>
{
    private readonly IBlogRepository _blogRepository;

    public GetBlogsByDealershipQueryHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<List<BlogDto>> Handle(GetBlogsByDealershipQuery request, CancellationToken cancellationToken)
    {
        var blogs = await _blogRepository.GetByDealershipIdAsync(request.DealershipId);
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
