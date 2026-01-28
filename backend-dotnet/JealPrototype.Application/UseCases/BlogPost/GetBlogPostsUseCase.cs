using AutoMapper;
using JealPrototype.Application.DTOs.BlogPost;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.BlogPost;

public class GetBlogPostsUseCase
{
    private readonly IBlogPostRepository _blogPostRepository;
    private readonly IMapper _mapper;

    public GetBlogPostsUseCase(IBlogPostRepository blogPostRepository, IMapper mapper)
    {
        _blogPostRepository = blogPostRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<BlogPostResponseDto>>> ExecuteAsync(
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var blogPosts = await _blogPostRepository.GetByDealershipIdAsync(dealershipId, cancellationToken);
        var response = _mapper.Map<List<BlogPostResponseDto>>(blogPosts);

        return ApiResponse<List<BlogPostResponseDto>>.SuccessResponse(response);
    }
}
