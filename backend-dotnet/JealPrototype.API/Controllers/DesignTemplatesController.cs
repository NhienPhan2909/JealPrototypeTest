using JealPrototype.API.Extensions;
using JealPrototype.API.Filters;
using JealPrototype.Application.DTOs.DesignTemplate;
using JealPrototype.Application.UseCases.DesignTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/design-templates")]
public class DesignTemplatesController : ControllerBase
{
    private readonly GetDesignTemplatesUseCase _getDesignTemplatesUseCase;
    private readonly CreateDesignTemplateUseCase _createDesignTemplateUseCase;
    private readonly DeleteDesignTemplateUseCase _deleteDesignTemplateUseCase;

    public DesignTemplatesController(
        GetDesignTemplatesUseCase getDesignTemplatesUseCase,
        CreateDesignTemplateUseCase createDesignTemplateUseCase,
        DeleteDesignTemplateUseCase deleteDesignTemplateUseCase)
    {
        _getDesignTemplatesUseCase = getDesignTemplatesUseCase;
        _createDesignTemplateUseCase = createDesignTemplateUseCase;
        _deleteDesignTemplateUseCase = deleteDesignTemplateUseCase;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DesignTemplateResponseDto>>> GetTemplates([FromQuery] int? dealershipId = null)
    {
        var templates = await _getDesignTemplatesUseCase.ExecuteAsync(dealershipId);
        return Ok(templates);
    }

    [HttpPost]
    [Authorize]
    [RequireDealershipAccess("DealershipId", DealershipAccessSource.Body, RequireAuthentication = true, AllowAdmin = true)]
    public async Task<ActionResult<DesignTemplateResponseDto>> CreateTemplate([FromBody] CreateDesignTemplateDto request)
    {
        var template = await _createDesignTemplateUseCase.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetTemplates), new { dealershipId = template.DealershipId }, template);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteTemplate(int id)
    {
        var success = await _deleteDesignTemplateUseCase.ExecuteAsync(id);
        if (!success)
            return NotFound(new { message = "Template not found or cannot be deleted (presets cannot be deleted)" });

        return NoContent();
    }
}
