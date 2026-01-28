using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.ValueObjects;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class DealershipConfiguration : IEntityTypeConfiguration<Dealership>
{
    public void Configure(EntityTypeBuilder<Dealership> builder)
    {
        builder.ToTable("dealership");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(d => d.LogoUrl)
            .HasColumnName("logo_url");

        builder.Property(d => d.Address)
            .IsRequired()
            .HasColumnName("address");

        builder.Property(d => d.Phone)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("phone");

        builder.Property(d => d.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("email")
            .HasConversion(
                email => email.Value,
                value => Email.Create(value));

        builder.Property(d => d.Hours)
            .HasColumnName("hours");

        builder.Property(d => d.About)
            .HasColumnName("about");

        builder.Property(d => d.WebsiteUrl)
            .HasMaxLength(255)
            .HasColumnName("website_url");

        builder.Property(d => d.FinancePolicy)
            .HasColumnName("finance_policy");

        builder.Property(d => d.WarrantyPolicy)
            .HasColumnName("warranty_policy");

        builder.Property(d => d.HeroBackgroundImage)
            .HasColumnName("hero_background_image");

        builder.Property(d => d.HeroType)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("hero_type")
            .HasConversion(
                ht => ht.ToString().ToLower(),
                ht => Enum.Parse<HeroType>(ht, true));

        builder.Property(d => d.HeroVideoUrl)
            .HasColumnName("hero_video_url");

        builder.Property(d => d.HeroCarouselImages)
            .HasColumnName("hero_carousel_images")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        builder.Property(d => d.ThemeColor)
            .IsRequired()
            .HasMaxLength(7)
            .HasColumnName("theme_color")
            .HasConversion(
                color => color.Value,
                value => HexColor.Create(value));

        builder.Property(d => d.SecondaryThemeColor)
            .IsRequired()
            .HasMaxLength(7)
            .HasColumnName("secondary_theme_color")
            .HasConversion(
                color => color.Value,
                value => HexColor.Create(value));

        builder.Property(d => d.BodyBackgroundColor)
            .IsRequired()
            .HasMaxLength(7)
            .HasColumnName("body_background_color")
            .HasConversion(
                color => color.Value,
                value => HexColor.Create(value));

        builder.Property(d => d.FontFamily)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("font_family");

        builder.Property(d => d.NavigationConfigJson)
            .HasColumnName("navigation_config")
            .HasColumnType("jsonb");

        builder.Property(d => d.FacebookUrl)
            .HasColumnName("facebook_url");

        builder.Property(d => d.InstagramUrl)
            .HasColumnName("instagram_url");

        builder.Property(d => d.FinancePromoImage)
            .HasColumnName("finance_promo_image");

        builder.Property(d => d.FinancePromoText)
            .HasMaxLength(500)
            .HasColumnName("finance_promo_text");

        builder.Property(d => d.WarrantyPromoImage)
            .HasColumnName("warranty_promo_image");

        builder.Property(d => d.WarrantyPromoText)
            .HasMaxLength(500)
            .HasColumnName("warranty_promo_text");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Ignore(d => d.UpdatedAt);

        builder.HasIndex(d => d.WebsiteUrl)
            .HasDatabaseName("idx_dealership_website_url")
            .IsUnique();

        builder.HasMany(d => d.Vehicles)
            .WithOne(v => v.Dealership)
            .HasForeignKey(v => v.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Leads)
            .WithOne(l => l.Dealership)
            .HasForeignKey(l => l.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Users)
            .WithOne(u => u.Dealership!)
            .HasForeignKey(u => u.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
