using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Dealership;
using JealPrototype.Application.UseCases.Dealership;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/dealers")]
[AllowAnonymous]
public class DealersController : ControllerBase
{
    private readonly GetDealershipUseCase _getDealershipUseCase;
    private readonly GetAllDealershipsUseCase _getAllDealershipsUseCase;
    private readonly UpdateDealershipUseCase _updateDealershipUseCase;

    public DealersController(
        GetDealershipUseCase getDealershipUseCase,
        GetAllDealershipsUseCase getAllDealershipsUseCase,
        UpdateDealershipUseCase updateDealershipUseCase)
    {
        _getDealershipUseCase = getDealershipUseCase;
        _getAllDealershipsUseCase = getAllDealershipsUseCase;
        _updateDealershipUseCase = updateDealershipUseCase;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DealershipResponseDto>>>> GetAllDealers()
    {
        var result = await _getAllDealershipsUseCase.ExecuteAsync();
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DealershipResponseDto>>> GetDealer(int id)
    {
        var result = await _getDealershipUseCase.ExecuteAsync(id);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DealershipResponseDto>>> UpdateDealer(int id, [FromBody] UpdateDealershipDto request)
    {
        var result = await _updateDealershipUseCase.ExecuteAsync(id, request);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
