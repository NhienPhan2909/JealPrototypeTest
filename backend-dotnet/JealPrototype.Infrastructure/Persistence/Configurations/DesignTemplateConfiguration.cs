using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class DesignTemplateConfiguration : IEntityTypeConfiguration<DesignTemplate>
{
    public void Configure(EntityTypeBuilder<DesignTemplate> builder)
    {
        builder.ToTable("design_templates");

        builder.HasKey(dt => dt.Id);
        builder.Property(dt => dt.Id).HasColumnName("id");

        builder.Property(dt => dt.DealershipId)
            .HasColumnName("dealership_id");

        builder.Property(dt => dt.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(dt => dt.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(dt => dt.ThemeColor)
            .IsRequired()
            .HasMaxLength(7)
            .HasColumnName("theme_color")
            .HasConversion(
                color => color.Value,
                value => HexColor.Create(value));

        builder.Property(dt => dt.SecondaryThemeColor)
            .IsRequired()
            .HasMaxLength(7)
            .HasColumnName("secondary_theme_color")
            .HasConversion(
                color => color.Value,
                value => HexColor.Create(value));

        builder.Property(dt => dt.BodyBackgroundColor)
            .IsRequired()
            .HasMaxLength(7)
            .HasColumnName("body_background_color")
            .HasConversion(
                color => color.Value,
                value => HexColor.Create(value));

        builder.Property(dt => dt.FontFamily)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("font_family");

        builder.Property(dt => dt.IsPreset)
            .HasColumnName("is_preset")
            .HasDefaultValue(false);

        builder.Property(dt => dt.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Ignore(dt => dt.UpdatedAt);

        builder.HasOne(dt => dt.Dealership)
            .WithMany()
            .HasForeignKey(dt => dt.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
