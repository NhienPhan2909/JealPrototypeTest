using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.UseCases.EasyCars;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JealPrototype.API.Controllers;

/// <summary>
/// Controller for managing EasyCars integration credentials
/// </summary>
[ApiController]
[Route("api/admin/easycars/credentials")]
[Authorize]
public class EasyCarsCredentialsController : ControllerBase
{
    private readonly CreateCredentialUseCase _createUseCase;
    private readonly GetCredentialUseCase _getUseCase;
    private readonly UpdateCredentialUseCase _updateUseCase;
    private readonly DeleteCredentialUseCase _deleteUseCase;
    private readonly ILogger<EasyCarsCredentialsController> _logger;

    public EasyCarsCredentialsController(
        CreateCredentialUseCase createUseCase,
        GetCredentialUseCase getUseCase,
        UpdateCredentialUseCase updateUseCase,
        DeleteCredentialUseCase deleteUseCase,
        ILogger<EasyCarsCredentialsController> logger)
    {
        _createUseCase = createUseCase ?? throw new ArgumentNullException(nameof(createUseCase));
        _getUseCase = getUseCase ?? throw new ArgumentNullException(nameof(getUseCase));
        _updateUseCase = updateUseCase ?? throw new ArgumentNullException(nameof(updateUseCase));
        _deleteUseCase = deleteUseCase ?? throw new ArgumentNullException(nameof(deleteUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates new EasyCars credentials for the authenticated dealership
    /// </summary>
    /// <param name="request">Credential creation request</param>
    /// <returns>Created credential metadata</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CredentialResponse>> CreateCredential(
        [FromBody] CreateCredentialRequest request)
    {
        try
        {
            var dealershipId = GetDealershipIdFromClaims();
            var response = await _createUseCase.ExecuteAsync(dealershipId, request);
            return CreatedAtAction(nameof(GetCredential), new { credentialId = response.Id }, response);
        }
        catch (DuplicateCredentialException ex)
        {
            _logger.LogWarning(ex, "Duplicate credential creation attempt");
            return Conflict(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Dealership not found during credential creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating credential");
            return StatusCode(500, new { error = "An error occurred while creating credentials" });
        }
    }

    /// <summary>
    /// Retrieves EasyCars credentials for the authenticated dealership
    /// </summary>
    /// <returns>Credential metadata (no secrets)</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CredentialMetadataResponse>> GetCredential()
    {
        try
        {
            var dealershipId = GetDealershipIdFromClaims();
            var response = await _getUseCase.ExecuteAsync(dealershipId);
            return Ok(response);
        }
        catch (CredentialNotFoundException ex)
        {
            _logger.LogInformation(ex, "Credentials not found for dealership");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving credential");
            return StatusCode(500, new { error = "An error occurred while retrieving credentials" });
        }
    }

    /// <summary>
    /// Updates EasyCars credentials for the authenticated dealership
    /// </summary>
    /// <param name="credentialId">ID of the credential to update</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated credential metadata</returns>
    [HttpPut("{credentialId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CredentialResponse>> UpdateCredential(
        int credentialId,
        [FromBody] UpdateCredentialRequest request)
    {
        try
        {
            var dealershipId = GetDealershipIdFromClaims();
            var response = await _updateUseCase.ExecuteAsync(dealershipId, credentialId, request);
            return Ok(response);
        }
        catch (CredentialNotFoundException ex)
        {
            _logger.LogWarning(ex, "Credential not found during update");
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized credential update attempt");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating credential");
            return StatusCode(500, new { error = "An error occurred while updating credentials" });
        }
    }

    /// <summary>
    /// Deletes EasyCars credentials for the authenticated dealership
    /// </summary>
    /// <param name="credentialId">ID of the credential to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{credentialId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCredential(int credentialId)
    {
        try
        {
            var dealershipId = GetDealershipIdFromClaims();
            await _deleteUseCase.ExecuteAsync(dealershipId, credentialId);
            return NoContent();
        }
        catch (CredentialNotFoundException ex)
        {
            _logger.LogWarning(ex, "Credential not found during deletion");
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized credential deletion attempt");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting credential");
            return StatusCode(500, new { error = "An error occurred while deleting credentials" });
        }
    }

    /// <summary>
    /// Extracts dealership ID from JWT claims
    /// </summary>
    private int GetDealershipIdFromClaims()
    {
        var dealershipIdClaim = User.FindFirst("DealershipId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(dealershipIdClaim) || !int.TryParse(dealershipIdClaim, out int dealershipId))
        {
            throw new UnauthorizedAccessException("Invalid or missing dealership ID in token");
        }

        return dealershipId;
    }
}
