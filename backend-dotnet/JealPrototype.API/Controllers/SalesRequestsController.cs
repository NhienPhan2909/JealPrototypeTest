using JealPrototype.API.Extensions;
using JealPrototype.API.Filters;
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
    private readonly DeleteSalesRequestUseCase _deleteSalesRequestUseCase;

    public SalesRequestsController(
        CreateSalesRequestUseCase createSalesRequestUseCase,
        GetSalesRequestsUseCase getSalesRequestsUseCase,
        UpdateSalesRequestStatusUseCase updateSalesRequestStatusUseCase,
        DeleteSalesRequestUseCase deleteSalesRequestUseCase)
    {
        _createSalesRequestUseCase = createSalesRequestUseCase;
        _getSalesRequestsUseCase = getSalesRequestsUseCase;
        _updateSalesRequestStatusUseCase = updateSalesRequestStatusUseCase;
        _deleteSalesRequestUseCase = deleteSalesRequestUseCase;
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
    [RequireDealershipAccess("dealershipId", DealershipAccessSource.Route, RequireAuthentication = true)]
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
    [RequireDealershipAccess("dealershipId", DealershipAccessSource.Query, RequireAuthentication = true)]
    public async Task<ActionResult<ApiResponse<SalesRequestResponseDto>>> UpdateSalesRequestStatus(int id, [FromQuery] int dealershipId, [FromBody] UpdateSalesRequestStatusDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<SalesRequestResponseDto>.ErrorResponse("Invalid user token"));

        var result = await _updateSalesRequestStatusUseCase.ExecuteAsync(id, dealershipId, request.Status);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [RequireDealershipAccess("dealershipId", DealershipAccessSource.Query, RequireAuthentication = true)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSalesRequest(int id, [FromQuery] int dealershipId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Invalid user token"));

        var result = await _deleteSalesRequestUseCase.ExecuteAsync(id, dealershipId);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
