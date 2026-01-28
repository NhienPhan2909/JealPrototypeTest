namespace JealPrototype.Application.DTOs.Dealership;

public class DealershipResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string Address { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Hours { get; set; }
    public string? About { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? FinancePolicy { get; set; }
    public string? WarrantyPolicy { get; set; }
    public string? HeroBackgroundImage { get; set; }
    public string HeroType { get; set; } = "image";
    public string? HeroVideoUrl { get; set; }
    public List<string> HeroCarouselImages { get; set; } = new();
    public string ThemeColor { get; set; } = "#3B82F6";
    public string SecondaryThemeColor { get; set; } = "#FFFFFF";
    public string BodyBackgroundColor { get; set; } = "#FFFFFF";
    public string FontFamily { get; set; } = "system";
    public string? NavigationConfig { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FinancePromoImage { get; set; }
    public string? FinancePromoText { get; set; }
    public string? WarrantyPromoImage { get; set; }
    public string? WarrantyPromoText { get; set; }
    public DateTime CreatedAt { get; set; }
}
