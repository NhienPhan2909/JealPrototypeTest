using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Dealership;
using JealPrototype.Application.UseCases.Dealership;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/dealerships")]
[Authorize]
public class DealershipsController : ControllerBase
{
    private readonly CreateDealershipUseCase _createDealershipUseCase;
    private readonly GetDealershipUseCase _getDealershipUseCase;
    private readonly GetAllDealershipsUseCase _getAllDealershipsUseCase;
    private readonly GetDealershipByUrlUseCase _getDealershipByUrlUseCase;
    private readonly UpdateDealershipUseCase _updateDealershipUseCase;
    private readonly DeleteDealershipUseCase _deleteDealershipUseCase;

    public DealershipsController(
        CreateDealershipUseCase createDealershipUseCase,
        GetDealershipUseCase getDealershipUseCase,
        GetAllDealershipsUseCase getAllDealershipsUseCase,
        GetDealershipByUrlUseCase getDealershipByUrlUseCase,
        UpdateDealershipUseCase updateDealershipUseCase,
        DeleteDealershipUseCase deleteDealershipUseCase)
    {
        _createDealershipUseCase = createDealershipUseCase;
        _getDealershipUseCase = getDealershipUseCase;
        _getAllDealershipsUseCase = getAllDealershipsUseCase;
        _getDealershipByUrlUseCase = getDealershipByUrlUseCase;
        _updateDealershipUseCase = updateDealershipUseCase;
        _deleteDealershipUseCase = deleteDealershipUseCase;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<DealershipResponseDto>>>> GetAllDealerships()
    {
        var result = await _getAllDealershipsUseCase.ExecuteAsync();
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DealershipResponseDto>>> CreateDealership([FromBody] CreateDealershipDto request)
    {
        var result = await _createDealershipUseCase.ExecuteAsync(request);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetDealership), new { id = result.Data!.Id }, result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<DealershipResponseDto>>> GetDealership(int id)
    {
        var result = await _getDealershipUseCase.ExecuteAsync(id);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("by-url/{websiteUrl}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<DealershipResponseDto>>> GetDealershipByUrl(string websiteUrl)
    {
        var result = await _getDealershipByUrlUseCase.ExecuteAsync(websiteUrl);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DealershipResponseDto>>> UpdateDealership(int id, [FromBody] UpdateDealershipDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<DealershipResponseDto>.ErrorResponse("Invalid user token"));

        var result = await _updateDealershipUseCase.ExecuteAsync(id, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDealership(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token"));

        var result = await _deleteDealershipUseCase.ExecuteAsync(id);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
