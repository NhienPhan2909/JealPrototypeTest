using JealPrototype.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for EasyCarsStockData entity
/// </summary>
public class EasyCarsStockDataConfiguration : IEntityTypeConfiguration<EasyCarsStockData>
{
    public void Configure(EntityTypeBuilder<EasyCarsStockData> builder)
    {
        builder.ToTable("easycar_stock_data");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        builder.Property(e => e.StockItemJson)
            .HasColumnName("stock_item_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.SyncedAt)
            .HasColumnName("synced_at")
            .IsRequired();

        builder.Property(e => e.ApiVersion)
            .HasColumnName("api_version")
            .HasMaxLength(20)
            .HasDefaultValue("1.0")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationship: One-to-One with Vehicle
        builder.HasOne(e => e.Vehicle)
            .WithMany()
            .HasForeignKey(e => e.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint on VehicleId
        builder.HasIndex(e => e.VehicleId)
            .IsUnique()
            .HasDatabaseName("idx_easycar_stock_data_vehicle_id_unique");

        // Index on synced_at for queries
        builder.HasIndex(e => e.SyncedAt)
            .HasDatabaseName("idx_easycar_stock_data_synced_at");
    }
}
