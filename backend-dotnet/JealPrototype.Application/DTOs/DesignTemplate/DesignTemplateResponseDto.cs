namespace JealPrototype.Application.DTOs.DesignTemplate;

public class DesignTemplateResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? DealershipId { get; set; }
    public bool IsPreset { get; set; }
    public string ThemeColor { get; set; } = null!;
    public string SecondaryThemeColor { get; set; } = null!;
    public string BodyBackgroundColor { get; set; } = null!;
    public string FontFamily { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
