using JealPrototype.Application.DTOs.Auth;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.UseCases.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly GetCurrentUserUseCase _getCurrentUserUseCase;

    public AuthController(LoginUseCase loginUseCase, GetCurrentUserUseCase getCurrentUserUseCase)
    {
        _loginUseCase = loginUseCase;
        _getCurrentUserUseCase = getCurrentUserUseCase;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _loginUseCase.ExecuteAsync(request);
        
        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Invalid user token"));

        var result = await _getCurrentUserUseCase.ExecuteAsync(userId);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(ApiResponse<object>.SuccessResponse(null, "Logged out successfully"));
    }
}
