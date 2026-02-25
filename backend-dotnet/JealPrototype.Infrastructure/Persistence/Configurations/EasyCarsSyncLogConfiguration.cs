using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for EasyCarsSyncLog entity (Story 2.3)
/// </summary>
public class EasyCarsSyncLogConfiguration : IEntityTypeConfiguration<EasyCarsSyncLog>
{
    public void Configure(EntityTypeBuilder<EasyCarsSyncLog> builder)
    {
        builder.ToTable("easycar_sync_logs");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(e => e.DealershipId)
            .HasColumnName("dealership_id")
            .IsRequired();

        builder.Property(e => e.SyncedAt)
            .HasColumnName("synced_at")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ItemsProcessed)
            .HasColumnName("items_processed")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.ItemsSucceeded)
            .HasColumnName("items_succeeded")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.ItemsFailed)
            .HasColumnName("items_failed")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.ErrorMessages)
            .HasColumnName("error_messages")
            .HasColumnType("jsonb")
            .IsRequired()
            .HasDefaultValue("[]");

        builder.Property(e => e.DurationMs)
            .HasColumnName("duration_ms")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.ApiVersion)
            .HasColumnName("api_version")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("1.0");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.Dealership)
            .WithMany()
            .HasForeignKey(e => e.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.DealershipId)
            .HasDatabaseName("idx_easycar_sync_logs_dealership_id");

        builder.HasIndex(e => e.SyncedAt)
            .HasDatabaseName("idx_easycar_sync_logs_synced_at");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_easycar_sync_logs_status");
    }
}
