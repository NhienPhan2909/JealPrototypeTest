using JealPrototype.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class DealershipSettingsConfiguration : IEntityTypeConfiguration<DealershipSettings>
{
    public void Configure(EntityTypeBuilder<DealershipSettings> builder)
    {
        builder.ToTable("dealership_settings");
        
        builder.HasKey(ds => ds.Id);
        builder.Property(ds => ds.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(ds => ds.DealershipId)
            .HasColumnName("dealership_id")
            .IsRequired();
        
        builder.Property(ds => ds.EasyCarAutoSyncEnabled)
            .HasColumnName("easycar_auto_sync_enabled")
            .HasDefaultValue(true)
            .IsRequired();
        
        builder.Property(ds => ds.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.Property(ds => ds.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        // Foreign key relationship
        builder.HasOne(ds => ds.Dealership)
            .WithMany()
            .HasForeignKey(ds => ds.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Unique constraint on dealership_id
        builder.HasIndex(ds => ds.DealershipId)
            .HasDatabaseName("idx_dealership_settings_dealership_id")
            .IsUnique();
        
        // Index for filtering by auto_sync_enabled
        builder.HasIndex(ds => ds.EasyCarAutoSyncEnabled)
            .HasDatabaseName("idx_dealership_settings_auto_sync")
            .HasFilter("easycar_auto_sync_enabled = TRUE");
    }
}
