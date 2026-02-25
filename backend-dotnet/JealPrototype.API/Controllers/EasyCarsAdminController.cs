using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.UseCases.EasyCars;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/admin/easycars")]
[Authorize]
public class EasyCarsAdminController : ControllerBase
{
    private readonly TestConnectionUseCase _testConnectionUseCase;
    private readonly ILogger<EasyCarsAdminController> _logger;

    public EasyCarsAdminController(
        TestConnectionUseCase testConnectionUseCase,
        ILogger<EasyCarsAdminController> logger)
    {
        _testConnectionUseCase = testConnectionUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Tests connection to EasyCars API with provided credentials
    /// </summary>
    /// <param name="request">Test connection request containing credentials and environment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test connection response with success/failure details</returns>
    [HttpPost("test-connection")]
    [ProducesResponseType(typeof(TestConnectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestConnection(
        [FromBody] TestConnectionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _testConnectionUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (TaskCanceledException)
        {
            return StatusCode(
                StatusCodes.Status408RequestTimeout,
                new { message = "Request was cancelled or timed out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in test connection endpoint");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error occurred" });
        }
    }
}
