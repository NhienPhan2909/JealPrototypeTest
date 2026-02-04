using AutoMapper;
using JealPrototype.Application.DTOs.BlogPost;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.BlogPost;

public class GetBlogPostBySlugUseCase
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;

    public GetBlogPostBySlugUseCase(IBlogPostRepository blogPostRepository, IMapper mapper)
    {
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<BlogPostResponseDto>> ExecuteAsync(
        string slug,
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var blogPosts = await _blogPostRepository.GetByDealershipIdAsync(dealershipId, cancellationToken);
        var blogPost = blogPosts.FirstOrDefault(bp => bp.Slug == slug);

        if (blogPost == null)
        {
            return ApiResponse<BlogPostResponseDto>.ErrorResponse("Blog post not found");
        }

        var response = _mapper.Map<BlogPostResponseDto>(blogPost);
        return ApiResponse<BlogPostResponseDto>.SuccessResponse(response);
    }
}
