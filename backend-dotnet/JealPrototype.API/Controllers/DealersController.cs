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
    private readonly CreateDealershipUseCase _createDealershipUseCase;
    private readonly DeleteDealershipUseCase _deleteDealershipUseCase;

    public DealersController(
        GetDealershipUseCase getDealershipUseCase,
        GetAllDealershipsUseCase getAllDealershipsUseCase,
        UpdateDealershipUseCase updateDealershipUseCase,
        CreateDealershipUseCase createDealershipUseCase,
        DeleteDealershipUseCase deleteDealershipUseCase)
    {
        _getDealershipUseCase = getDealershipUseCase;
        _getAllDealershipsUseCase = getAllDealershipsUseCase;
        _updateDealershipUseCase = updateDealershipUseCase;
        _createDealershipUseCase = createDealershipUseCase;
        _deleteDealershipUseCase = deleteDealershipUseCase;
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

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<DealershipResponseDto>>> CreateDealer([FromBody] CreateDealershipDto request)
    {
        var result = await _createDealershipUseCase.ExecuteAsync(request);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetDealer), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DealershipResponseDto>>> UpdateDealer(int id, [FromBody] UpdateDealershipDto request)
    {
        var result = await _updateDealershipUseCase.ExecuteAsync(id, request);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDealer(int id)
    {
        var result = await _deleteDealershipUseCase.ExecuteAsync(id);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
