using JealPrototype.API.Extensions;
using JealPrototype.API.Filters;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.User;
using JealPrototype.Application.UseCases.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly GetUserByIdUseCase _getUserByIdUseCase;
    private readonly GetDealershipUsersUseCase _getDealershipUsersUseCase;
    private readonly UpdateUserUseCase _updateUserUseCase;
    private readonly DeleteUserUseCase _deleteUserUseCase;

    public UsersController(
        CreateUserUseCase createUserUseCase,
        GetUsersUseCase getUsersUseCase,
        GetUserByIdUseCase getUserByIdUseCase,
        GetDealershipUsersUseCase getDealershipUsersUseCase,
        UpdateUserUseCase updateUserUseCase,
        DeleteUserUseCase deleteUserUseCase)
    {
        _createUserUseCase = createUserUseCase;
        _getUsersUseCase = getUsersUseCase;
        _getUserByIdUseCase = getUserByIdUseCase;
        _getDealershipUsersUseCase = getDealershipUsersUseCase;
        _updateUserUseCase = updateUserUseCase;
        _deleteUserUseCase = deleteUserUseCase;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> CreateUser([FromBody] CreateUserDto request)
    {
        var creatorId = User.GetUserId();
        var result = await _createUserUseCase.ExecuteAsync(request, creatorId);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result);
    }

    [HttpGet("dealership/{dealershipId}/all")]
    [RequireDealershipAccess("dealershipId", DealershipAccessSource.Route, RequireAuthentication = true)]
    public async Task<ActionResult<ApiResponse<List<UserResponseDto>>>> GetUsers(int dealershipId)
    {
        var requestorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(requestorIdClaim) || !int.TryParse(requestorIdClaim, out int requestorId))
            return Unauthorized(ApiResponse<List<UserResponseDto>>.ErrorResponse("Invalid user token"));

        var result = await _getUsersUseCase.ExecuteAsync(requestorId, dealershipId);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(int id)
    {
        var result = await _getUserByIdUseCase.ExecuteAsync(id);
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("dealership/{dealershipId}")]
    [RequireDealershipAccess("dealershipId", DealershipAccessSource.Route, RequireAuthentication = true)]
    public async Task<ActionResult<ApiResponse<List<UserResponseDto>>>> GetDealershipUsers(int dealershipId)
    {
        var requestorId = User.GetUserId();
        var result = await _getDealershipUsersUseCase.ExecuteAsync(dealershipId, requestorId);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUser(int id, [FromBody] UpdateUserDto request)
    {
        var updaterId = User.GetUserId();
        var result = await _updateUserUseCase.ExecuteAsync(id, request, updaterId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
    {
        var deleterIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(deleterIdClaim) || !int.TryParse(deleterIdClaim, out int deleterId))
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token"));

        var result = await _deleteUserUseCase.ExecuteAsync(id, deleterId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
