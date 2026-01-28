using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.SalesRequest;
using JealPrototype.Application.UseCases.SalesRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/sales-requests")]
public class SalesRequestsController : ControllerBase
{
    private readonly CreateSalesRequestUseCase _createSalesRequestUseCase;
    private readonly GetSalesRequestsUseCase _getSalesRequestsUseCase;
    private readonly UpdateSalesRequestStatusUseCase _updateSalesRequestStatusUseCase;

    public SalesRequestsController(
        CreateSalesRequestUseCase createSalesRequestUseCase,
        GetSalesRequestsUseCase getSalesRequestsUseCase,
        UpdateSalesRequestStatusUseCase updateSalesRequestStatusUseCase)
    {
        _createSalesRequestUseCase = createSalesRequestUseCase;
        _getSalesRequestsUseCase = getSalesRequestsUseCase;
        _updateSalesRequestStatusUseCase = updateSalesRequestStatusUseCase;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SalesRequestResponseDto>>> CreateSalesRequest([FromBody] CreateSalesRequestDto request)
    {
        var result = await _createSalesRequestUseCase.ExecuteAsync(request.DealershipId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetSalesRequests), new { dealershipId = request.DealershipId }, result);
    }

    [HttpGet("dealership/{dealershipId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<SalesRequestResponseDto>>>> GetSalesRequests(int dealershipId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<List<SalesRequestResponseDto>>.ErrorResponse("Invalid user token"));

        var result = await _getSalesRequestsUseCase.ExecuteAsync(dealershipId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SalesRequestResponseDto>>> UpdateSalesRequestStatus(int id, [FromBody] UpdateSalesRequestStatusDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<SalesRequestResponseDto>.ErrorResponse("Invalid user token"));

        // Get user's dealership ID from somewhere (this should be enhanced with proper authorization)
        // For now, we'll use a simplified approach - in production, fetch user entity to get dealership
        var result = await _updateSalesRequestStatusUseCase.ExecuteAsync(id, userId, request.Status);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
