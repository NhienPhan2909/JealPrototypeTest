using JealPrototype.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class SystemSettingsConfiguration : IEntityTypeConfiguration<SystemSettings>
{
    public void Configure(EntityTypeBuilder<SystemSettings> builder)
    {
        builder.ToTable("system_settings");
        
        builder.HasKey(s => s.Key);
        builder.Property(s => s.Key)
            .HasColumnName("key")
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(s => s.Value)
            .HasColumnName("value")
            .IsRequired();
        
        builder.Property(s => s.Description)
            .HasColumnName("description");
        
        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        // Seed default settings
        builder.HasData(
            SystemSettings.Create("easycar_sync_enabled", "true", "Global toggle for all EasyCars stock synchronization"),
            SystemSettings.Create("easycar_sync_cron", "0 2 * * *", "Cron expression for sync schedule (default: 2 AM daily)"),
            SystemSettings.Create("easycar_sync_concurrency", "1", "Max concurrent dealership syncs (1=sequential)"),
            SystemSettings.Create("easycar_image_sync_enabled", "true", "Enable/disable image sync during stock sync (default: true)")
        );
    }
}
