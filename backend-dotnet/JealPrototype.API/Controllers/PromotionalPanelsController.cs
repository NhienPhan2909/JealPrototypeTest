using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JealPrototype.Application.DTOs.PromotionalPanel;
using JealPrototype.Application.UseCases.PromotionalPanel.Commands;
using JealPrototype.Application.UseCases.PromotionalPanel.Queries;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionalPanelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PromotionalPanelsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dealership/{dealershipId}")]
    public async Task<ActionResult<List<PromotionalPanelDto>>> GetByDealership(int dealershipId)
    {
        var panels = await _mediator.Send(new GetPromotionalPanelsByDealershipQuery(dealershipId));
        return Ok(panels);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PromotionalPanelDto>> Create([FromBody] CreatePromotionalPanelDto dto)
    {
        var panel = await _mediator.Send(new CreatePromotionalPanelCommand(dto));
        return Ok(panel);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PromotionalPanelDto>> Update(int id, [FromBody] UpdatePromotionalPanelDto dto)
    {
        var panel = await _mediator.Send(new UpdatePromotionalPanelCommand(id, dto));
        if (panel == null)
            return NotFound();
        return Ok(panel);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeletePromotionalPanelCommand(id));
        if (!result)
            return NotFound();
        return NoContent();
    }
}
