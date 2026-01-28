using JealPrototype.Domain.Enums;
using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Domain.Entities;

public class Dealership : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? LogoUrl { get; private set; }
    public string Address { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string? Hours { get; private set; }
    public string? About { get; private set; }
    public string? WebsiteUrl { get; private set; }
    
    public string? FinancePolicy { get; private set; }
    public string? WarrantyPolicy { get; private set; }
    
    public string? HeroBackgroundImage { get; private set; }
    public HeroType HeroType { get; private set; } = HeroType.Image;
    public string? HeroVideoUrl { get; private set; }
    public List<string> HeroCarouselImages { get; private set; } = new();
    
    public HexColor ThemeColor { get; private set; } = null!;
    public HexColor SecondaryThemeColor { get; private set; } = null!;
    public HexColor BodyBackgroundColor { get; private set; } = null!;
    public string FontFamily { get; private set; } = "system";
    
    public string? NavigationConfigJson { get; private set; }
    
    public string? FacebookUrl { get; private set; }
    public string? InstagramUrl { get; private set; }
    
    public string? FinancePromoImage { get; private set; }
    public string? FinancePromoText { get; private set; }
    public string? WarrantyPromoImage { get; private set; }
    public string? WarrantyPromoText { get; private set; }

    public ICollection<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();
    public ICollection<Lead> Leads { get; private set; } = new List<Lead>();
    public ICollection<User> Users { get; private set; } = new List<User>();

    private Dealership() { }

    public static Dealership Create(
        string name,
        string address,
        string phone,
        string email,
        string? logoUrl = null,
        string? hours = null,
        string? about = null,
        string? websiteUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 255)
            throw new ArgumentException("Name is required and must be 255 characters or less");

        if (string.IsNullOrWhiteSpace(address) || address.Length > 255)
            throw new ArgumentException("Address is required and must be 255 characters or less");

        if (string.IsNullOrWhiteSpace(phone) || phone.Length > 20)
            throw new ArgumentException("Phone is required and must be 20 characters or less");

        return new Dealership
        {
            Name = name,
            Address = address,
            Phone = phone,
            Email = Email.Create(email),
            LogoUrl = logoUrl,
            Hours = hours,
            About = about,
            WebsiteUrl = websiteUrl,
            ThemeColor = HexColor.Create("#3B82F6"),
            SecondaryThemeColor = HexColor.Create("#FFFFFF"),
            BodyBackgroundColor = HexColor.Create("#FFFFFF")
        };
    }

    public void Update(
        string name,
        string address,
        string phone,
        string email,
        string? logoUrl = null,
        string? hours = null,
        string? about = null,
        string? websiteUrl = null)
    {
        Name = name;
        Address = address;
        Phone = phone;
        Email = Email.Create(email);
        LogoUrl = logoUrl;
        Hours = hours;
        About = about;
        WebsiteUrl = websiteUrl;
    }

    public void UpdateBranding(string themeColor, string secondaryThemeColor, string bodyBackgroundColor, string fontFamily)
    {
        ThemeColor = HexColor.Create(themeColor);
        SecondaryThemeColor = HexColor.Create(secondaryThemeColor);
        BodyBackgroundColor = HexColor.Create(bodyBackgroundColor);
        FontFamily = fontFamily;
    }

    public void UpdateFinancePolicy(string? policy) => FinancePolicy = policy;
    public void UpdateWarrantyPolicy(string? policy) => WarrantyPolicy = policy;
    public void UpdateHeroSettings(HeroType heroType, string? backgroundImage = null, string? videoUrl = null, List<string>? carouselImages = null)
    {
        HeroType = heroType;
        HeroBackgroundImage = backgroundImage;
        HeroVideoUrl = videoUrl;
        if (carouselImages != null) HeroCarouselImages = carouselImages;
    }
    public void UpdateNavigationConfig(string? config) => NavigationConfigJson = config;
    public void UpdateSocialMedia(string? facebookUrl, string? instagramUrl)
    {
        FacebookUrl = facebookUrl;
        InstagramUrl = instagramUrl;
    }
    public void UpdateFinancePromo(string? image, string? text)
    {
        FinancePromoImage = image;
        FinancePromoText = text;
    }
    public void UpdateWarrantyPromo(string? image, string? text)
    {
        WarrantyPromoImage = image;
        WarrantyPromoText = text;
    }
}
