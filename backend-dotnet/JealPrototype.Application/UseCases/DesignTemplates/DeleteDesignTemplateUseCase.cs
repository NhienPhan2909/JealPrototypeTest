using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.DesignTemplates;

public class DeleteDesignTemplateUseCase
{
    private readonly IDesignTemplateRepository _repository;

    public DeleteDesignTemplateUseCase(IDesignTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(int id)
    {
        var template = await _repository.GetByIdAsync(id);
        if (template == null || template.IsPreset)
        {
            return false; // Cannot delete presets or non-existent templates
        }

        await _repository.DeleteAsync(template);
        return true;
    }
}
