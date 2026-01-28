using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Domain.Entities;

public class DesignTemplate : BaseEntity
{
    public int? DealershipId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public HexColor ThemeColor { get; private set; } = null!;
    public HexColor SecondaryThemeColor { get; private set; } = null!;
    public HexColor BodyBackgroundColor { get; private set; } = null!;
    public string FontFamily { get; private set; } = null!;
    public bool IsPreset { get; private set; }

    public Dealership? Dealership { get; private set; }

    private DesignTemplate() { }

    public static DesignTemplate Create(
        string name,
        string themeColor,
        string secondaryThemeColor,
        string bodyBackgroundColor,
        string fontFamily,
        int? dealershipId = null,
        string? description = null,
        bool isPreset = false)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            throw new ArgumentException("Name is required and must be 100 characters or less", nameof(name));

        if (string.IsNullOrWhiteSpace(fontFamily) || fontFamily.Length > 100)
            throw new ArgumentException("Font family is required and must be 100 characters or less", nameof(fontFamily));

        return new DesignTemplate
        {
            Name = name,
            Description = description,
            ThemeColor = HexColor.Create(themeColor),
            SecondaryThemeColor = HexColor.Create(secondaryThemeColor),
            BodyBackgroundColor = HexColor.Create(bodyBackgroundColor),
            FontFamily = fontFamily,
            DealershipId = dealershipId,
            IsPreset = isPreset
        };
    }
}
