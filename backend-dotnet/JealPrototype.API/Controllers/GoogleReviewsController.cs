using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JealPrototype.Application.Interfaces;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/google-reviews")]
public class GoogleReviewsController : ControllerBase
{
    private readonly IGoogleReviewsService _googleReviewsService;
    private readonly ILogger<GoogleReviewsController> _logger;

    public GoogleReviewsController(
        IGoogleReviewsService googleReviewsService,
        ILogger<GoogleReviewsController> logger)
    {
        _googleReviewsService = googleReviewsService;
        _logger = logger;
    }

    [HttpGet("{dealershipId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviews(int dealershipId)
    {
        try
        {
            if (dealershipId <= 0)
            {
                return BadRequest(new { error = "Invalid dealership ID" });
            }

            var result = await _googleReviewsService.GetReviewsAsync(dealershipId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Google reviews for dealership {DealershipId}", dealershipId);
            return Ok(new
            {
                reviews = new object[0],
                googleMapsUrl = "",
                message = "Unable to load reviews at this time"
            });
        }
    }
}
