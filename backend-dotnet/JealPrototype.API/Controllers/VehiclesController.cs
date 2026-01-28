using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Vehicle;
using JealPrototype.Application.UseCases.Vehicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly CreateVehicleUseCase _createVehicleUseCase;
    private readonly GetVehiclesUseCase _getVehiclesUseCase;
    private readonly GetVehicleByIdUseCase _getVehicleByIdUseCase;
    private readonly GetDealershipVehiclesUseCase _getDealershipVehiclesUseCase;
    private readonly UpdateVehicleUseCase _updateVehicleUseCase;
    private readonly DeleteVehicleUseCase _deleteVehicleUseCase;

    public VehiclesController(
        CreateVehicleUseCase createVehicleUseCase,
        GetVehiclesUseCase getVehiclesUseCase,
        GetVehicleByIdUseCase getVehicleByIdUseCase,
        GetDealershipVehiclesUseCase getDealershipVehiclesUseCase,
        UpdateVehicleUseCase updateVehicleUseCase,
        DeleteVehicleUseCase deleteVehicleUseCase)
    {
        _createVehicleUseCase = createVehicleUseCase;
        _getVehiclesUseCase = getVehiclesUseCase;
        _getVehicleByIdUseCase = getVehicleByIdUseCase;
        _getDealershipVehiclesUseCase = getDealershipVehiclesUseCase;
        _updateVehicleUseCase = updateVehicleUseCase;
        _deleteVehicleUseCase = deleteVehicleUseCase;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> CreateVehicle([FromBody] CreateVehicleDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<VehicleResponseDto>.ErrorResponse("Invalid user token"));

        var result = await _createVehicleUseCase.ExecuteAsync(request.DealershipId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetVehicle), new { id = result.Data!.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<VehicleResponseDto>>> GetVehicles([FromQuery] int dealershipId, [FromQuery] VehicleFilterDto filter)
    {
        var result = await _getVehiclesUseCase.ExecuteAsync(dealershipId, filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> GetVehicle(int id, [FromQuery] int dealershipId)
    {
        var result = await _getVehicleByIdUseCase.ExecuteAsync(id, dealershipId);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("dealership/{dealershipId}")]
    public async Task<ActionResult<PagedResponse<VehicleResponseDto>>> GetDealershipVehicles(int dealershipId, [FromQuery] VehicleFilterDto filter)
    {
        var result = await _getVehiclesUseCase.ExecuteAsync(dealershipId, filter);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> UpdateVehicle(int id, [FromQuery] int dealershipId, [FromBody] UpdateVehicleDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<VehicleResponseDto>.ErrorResponse("Invalid user token"));

        var result = await _updateVehicleUseCase.ExecuteAsync(id, dealershipId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVehicle(int id, [FromQuery] int dealershipId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token"));

        var result = await _deleteVehicleUseCase.ExecuteAsync(id, dealershipId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
