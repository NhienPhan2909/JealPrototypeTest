using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class PromotionalPanelConfiguration : IEntityTypeConfiguration<PromotionalPanel>
{
    public void Configure(EntityTypeBuilder<PromotionalPanel> builder)
    {
        builder.ToTable("promotional_panel");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        
        builder.Property(p => p.DealershipId).HasColumnName("dealership_id").IsRequired();
        builder.Property(p => p.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
        builder.Property(p => p.Description).HasColumnName("description").IsRequired();
        builder.Property(p => p.ImageUrl).HasColumnName("image_url");
        builder.Property(p => p.LinkUrl).HasColumnName("link_url");
        builder.Property(p => p.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        
        builder.Ignore(p => p.UpdatedAt);
    }
}
