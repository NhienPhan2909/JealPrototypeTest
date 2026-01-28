using AutoMapper;
using JealPrototype.Application.DTOs.BlogPost;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.BlogPost;

public class GetBlogPostByIdUseCase
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;

    public GetBlogPostByIdUseCase(IBlogPostRepository blogPostRepository, IMapper mapper)
    {
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<BlogPostResponseDto>> ExecuteAsync(
        int blogPostId,
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var blogPost = await _blogPostRepository.GetByIdAsync(blogPostId, cancellationToken);

        if (blogPost == null || blogPost.DealershipId != dealershipId)
        {
            return ApiResponse<BlogPostResponseDto>.ErrorResponse("Blog post not found");
        }

        var response = _mapper.Map<BlogPostResponseDto>(blogPost);
        return ApiResponse<BlogPostResponseDto>.SuccessResponse(response);
    }
}
