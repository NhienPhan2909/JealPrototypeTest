using JealPrototype.Application.DTOs.BlogPost;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.UseCases.BlogPost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/blog-posts")]
public class BlogPostsController : ControllerBase
{
    private readonly CreateBlogPostUseCase _createBlogPostUseCase;
    private readonly GetBlogPostsUseCase _getBlogPostsUseCase;
    private readonly GetBlogPostByIdUseCase _getBlogPostByIdUseCase;
    private readonly UpdateBlogPostUseCase _updateBlogPostUseCase;
    private readonly DeleteBlogPostUseCase _deleteBlogPostUseCase;

    public BlogPostsController(
        CreateBlogPostUseCase createBlogPostUseCase,
        GetBlogPostsUseCase getBlogPostsUseCase,
        GetBlogPostByIdUseCase getBlogPostByIdUseCase,
        UpdateBlogPostUseCase updateBlogPostUseCase,
        DeleteBlogPostUseCase deleteBlogPostUseCase)
    {
        _createBlogPostUseCase = createBlogPostUseCase;
        _getBlogPostsUseCase = getBlogPostsUseCase;
        _getBlogPostByIdUseCase = getBlogPostByIdUseCase;
        _updateBlogPostUseCase = updateBlogPostUseCase;
        _deleteBlogPostUseCase = deleteBlogPostUseCase;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<BlogPostResponseDto>>> CreateBlogPost([FromBody] CreateBlogPostDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<BlogPostResponseDto>.ErrorResponse("Invalid user token"));

        var result = await _createBlogPostUseCase.ExecuteAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetBlogPost), new { id = result.Data!.Id }, result);
    }

    [HttpGet("dealership/{dealershipId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<BlogPostResponseDto>>>> GetBlogPosts(
        int dealershipId, 
        [FromQuery] bool publishedOnly = false)
    {
        var result = await _getBlogPostsUseCase.ExecuteAsync(dealershipId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<BlogPostResponseDto>>> GetBlogPost(int id, [FromQuery] int dealershipId)
    {
        var result = await _getBlogPostByIdUseCase.ExecuteAsync(id, dealershipId);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<BlogPostResponseDto>>> UpdateBlogPost(int id, [FromBody] UpdateBlogPostDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<BlogPostResponseDto>.ErrorResponse("Invalid user token"));

        var result = await _updateBlogPostUseCase.ExecuteAsync(id, userId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBlogPost(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token"));

        var result = await _deleteBlogPostUseCase.ExecuteAsync(id, userId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
