using MediatR;
using Microsoft.AspNetCore.Mvc;
using JealPrototype.Application.DTOs.HeroMedia;
using JealPrototype.Application.UseCases.HeroMedia.Queries;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HeroMediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public HeroMediaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dealership/{dealershipId}")]
    public async Task<ActionResult<HeroMediaDto>> GetByDealership(int dealershipId)
    {
        var heroMedia = await _mediator.Send(new GetHeroMediaByDealershipQuery(dealershipId));
        if (heroMedia == null)
            return NotFound();
        return Ok(heroMedia);
    }
}
