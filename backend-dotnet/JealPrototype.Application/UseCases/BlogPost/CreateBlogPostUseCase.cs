using AutoMapper;
using JealPrototype.Application.DTOs.BlogPost;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.BlogPost;

public class CreateBlogPostUseCase
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;

    public CreateBlogPostUseCase(IBlogPostRepository blogPostRepository, IMapper mapper)
    {
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<BlogPostResponseDto>> ExecuteAsync(
        int dealershipId,
        CreateBlogPostDto request,
        CancellationToken cancellationToken = default)
    {
        var status = request.Status.ToLower() switch
        {
            "draft" => BlogPostStatus.Draft,
            "published" => BlogPostStatus.Published,
            "archived" => BlogPostStatus.Archived,
            _ => BlogPostStatus.Draft
        };

        var blogPost = Domain.Entities.BlogPost.Create(
            dealershipId,
            request.Title,
            request.Content,
            request.AuthorName,
            request.Slug,
            request.Excerpt,
            request.FeaturedImageUrl,
            status);

        var createdBlogPost = await _blogPostRepository.AddAsync(blogPost, cancellationToken);
        var response = _mapper.Map<BlogPostResponseDto>(createdBlogPost);

        return ApiResponse<BlogPostResponseDto>.SuccessResponse(response, "Blog post created successfully");
    }
}
