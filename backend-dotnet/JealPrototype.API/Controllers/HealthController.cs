using Microsoft.AspNetCore.Mvc;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "JealPrototype.API",
            version = "1.0.0"
        });
    }
}
