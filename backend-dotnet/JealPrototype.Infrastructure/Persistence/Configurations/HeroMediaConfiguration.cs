using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class HeroMediaConfiguration : IEntityTypeConfiguration<HeroMedia>
{
    public void Configure(EntityTypeBuilder<HeroMedia> builder)
    {
        builder.ToTable("hero_media");
        
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");
        
        builder.Property(h => h.DealershipId).HasColumnName("dealership_id").IsRequired();
        builder.Property(h => h.MediaType).HasColumnName("media_type").IsRequired().HasMaxLength(50);
        builder.Property(h => h.MediaUrl).HasColumnName("media_url");
        builder.Property(h => h.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        
        builder.Ignore(h => h.UpdatedAt);
    }
}
