using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Lead;
using JealPrototype.Application.UseCases.Lead;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/leads")]
public class LeadsController : ControllerBase
{
    private readonly CreateLeadUseCase _createLeadUseCase;
    private readonly GetLeadsUseCase _getLeadsUseCase;
    private readonly UpdateLeadStatusUseCase _updateLeadStatusUseCase;

    public LeadsController(
        CreateLeadUseCase createLeadUseCase,
        GetLeadsUseCase getLeadsUseCase,
        UpdateLeadStatusUseCase updateLeadStatusUseCase)
    {
        _createLeadUseCase = createLeadUseCase;
        _getLeadsUseCase = getLeadsUseCase;
        _updateLeadStatusUseCase = updateLeadStatusUseCase;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeadResponseDto>>> CreateLead([FromBody] CreateLeadDto request)
    {
        var result = await _createLeadUseCase.ExecuteAsync(request.DealershipId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetLeads), new { dealershipId = request.DealershipId }, result);
    }

    [HttpGet("dealership/{dealershipId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<LeadResponseDto>>>> GetLeads(int dealershipId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<List<LeadResponseDto>>.ErrorResponse("Invalid user token"));

        var result = await _getLeadsUseCase.ExecuteAsync(dealershipId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<LeadResponseDto>>> UpdateLeadStatus(int id, [FromBody] UpdateLeadStatusDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<LeadResponseDto>.ErrorResponse("Invalid user token"));

        var result = await _updateLeadStatusUseCase.ExecuteAsync(id, userId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
