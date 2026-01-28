using MediatR;
using JealPrototype.Application.DTOs.Blog;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Blog.Commands;

public record UpdateBlogCommand(int Id, UpdateBlogDto Dto) : IRequest<BlogDto?>;

public class UpdateBlogCommandHandler : IRequestHandler<UpdateBlogCommand, BlogDto?>
{
    private readonly IBlogRepository _blogRepository;

    public UpdateBlogCommandHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<BlogDto?> Handle(UpdateBlogCommand request, CancellationToken cancellationToken)
    {
        var blog = await _blogRepository.GetByIdAsync(request.Id);
        if (blog == null)
            return null;

        if (request.Dto.Title != null)
            blog.Title = request.Dto.Title;
        if (request.Dto.Content != null)
            blog.Content = request.Dto.Content;
        if (request.Dto.ImageUrl != null)
            blog.ImageUrl = request.Dto.ImageUrl;
        if (request.Dto.PublishedDate.HasValue)
            blog.PublishedDate = request.Dto.PublishedDate.Value;
        if (request.Dto.Author != null)
            blog.Author = request.Dto.Author;

        _blogRepository.Update(blog);
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
