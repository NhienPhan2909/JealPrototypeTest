using AutoMapper;
using JealPrototype.Application.DTOs.BlogPost;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.BlogPost;

public class UpdateBlogPostUseCase
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;

    public UpdateBlogPostUseCase(IBlogPostRepository blogPostRepository, IMapper mapper)
    {
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<BlogPostResponseDto>> ExecuteAsync(
        int blogPostId,
        int dealershipId,
        UpdateBlogPostDto request,
        CancellationToken cancellationToken = default)
    {
        var blogPost = await _blogPostRepository.GetByIdAsync(blogPostId, cancellationToken);

        if (blogPost == null || blogPost.DealershipId != dealershipId)
        {
            return ApiResponse<BlogPostResponseDto>.ErrorResponse("Blog post not found");
        }

        var status = !string.IsNullOrWhiteSpace(request.Status)
            ? request.Status.ToLower() switch
            {
                "draft" => BlogPostStatus.Draft,
                "published" => BlogPostStatus.Published,
                "archived" => BlogPostStatus.Archived,
                _ => blogPost.Status
            }
            : blogPost.Status;

        blogPost.Update(
            request.Title ?? blogPost.Title,
            request.Content ?? blogPost.Content,
            request.Excerpt ?? blogPost.Excerpt,
            request.FeaturedImageUrl ?? blogPost.FeaturedImageUrl,
            status != blogPost.Status ? status : null);

        await _blogPostRepository.UpdateAsync(blogPost, cancellationToken);

        var response = _mapper.Map<BlogPostResponseDto>(blogPost);
        return ApiResponse<BlogPostResponseDto>.SuccessResponse(response, "Blog post updated successfully");
    }
}
