using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.BlogPost;

public class DeleteBlogPostUseCase
{
    private readonly IBlogPostRepository _blogPostRepository;

    public DeleteBlogPostUseCase(IBlogPostRepository blogPostRepository)
    {
        _blogPostRepository = blogPostRepository;
    }

    public async Task<ApiResponse<bool>> ExecuteAsync(
        int blogPostId,
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var blogPost = await _blogPostRepository.GetByIdAsync(blogPostId, cancellationToken);

        if (blogPost == null || blogPost.DealershipId != dealershipId)
        {
            return ApiResponse<bool>.ErrorResponse("Blog post not found");
        }

        await _blogPostRepository.DeleteAsync(blogPost, cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Blog post deleted successfully");
    }
}
