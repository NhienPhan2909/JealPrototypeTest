using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicle");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");

        builder.Property(v => v.DealershipId)
            .IsRequired()
            .HasColumnName("dealership_id");

        builder.Property(v => v.Make)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("make");

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("model");

        builder.Property(v => v.Year)
            .IsRequired()
            .HasColumnName("year");

        builder.Property(v => v.Price)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasColumnName("price");

        builder.Property(v => v.Mileage)
            .IsRequired()
            .HasColumnName("mileage");

        builder.Property(v => v.Condition)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnName("condition")
            .HasConversion(
                c => c.ToString().ToLower(),
                c => Enum.Parse<VehicleCondition>(c, true));

        builder.Property(v => v.Status)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnName("status")
            .HasConversion(
                s => s.ToString().ToLower(),
                s => Enum.Parse<VehicleStatus>(s, true));

        builder.Property(v => v.Title)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("title");

        builder.Property(v => v.Description)
            .HasColumnName("description");

        builder.Property(v => v.Images)
            .HasColumnName("images")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        // EasyCars Integration Fields
        builder.Property(v => v.EasyCarsStockNumber)
            .HasMaxLength(100)
            .HasColumnName("easycars_stock_number");

        builder.Property(v => v.EasyCarsYardCode)
            .HasMaxLength(50)
            .HasColumnName("easycars_yard_code");

        builder.Property(v => v.EasyCarsVIN)
            .HasMaxLength(17)
            .HasColumnName("easycars_vin");

        builder.Property(v => v.EasyCarsRawData)
            .HasColumnName("easycars_raw_data")
            .HasColumnType("jsonb");

        builder.Property(v => v.DataSource)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("data_source")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<DataSource>(v))
            .HasDefaultValue(DataSource.Manual);

        builder.Property(v => v.LastSyncedFromEasyCars)
            .HasColumnName("last_synced_from_easycars");

        builder.Property(v => v.ExteriorColor)
            .HasMaxLength(50)
            .HasColumnName("exterior_color");

        builder.Property(v => v.InteriorColor)
            .HasMaxLength(50)
            .HasColumnName("interior_color");

        builder.Property(v => v.Body)
            .HasMaxLength(50)
            .HasColumnName("body");

        builder.Property(v => v.FuelType)
            .HasMaxLength(50)
            .HasColumnName("fuel_type");

        builder.Property(v => v.GearType)
            .HasMaxLength(50)
            .HasColumnName("gear_type");

        builder.Property(v => v.EngineCapacity)
            .HasMaxLength(50)
            .HasColumnName("engine_capacity");

        builder.Property(v => v.DoorCount)
            .HasColumnName("door_count");

        builder.Property(v => v.Features)
            .HasColumnName("features")
            .HasColumnType("jsonb");

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Ignore(v => v.UpdatedAt);

        builder.HasIndex(v => v.DealershipId)
            .HasDatabaseName("idx_vehicle_dealership_id");

        builder.HasIndex(v => v.Status)
            .HasDatabaseName("idx_vehicle_status");

        builder.HasIndex(v => new { v.DealershipId, v.Status })
            .HasDatabaseName("idx_vehicle_dealership_status");

        // EasyCars indexes
        builder.HasIndex(v => v.EasyCarsStockNumber)
            .HasDatabaseName("idx_vehicles_easycars_stock")
            .HasFilter("easycars_stock_number IS NOT NULL");

        builder.HasIndex(v => v.DataSource)
            .HasDatabaseName("idx_vehicles_data_source");

        builder.HasIndex(v => v.EasyCarsVIN)
            .HasDatabaseName("idx_vehicles_easycars_vin")
            .HasFilter("easycars_vin IS NOT NULL");

        // CHECK constraint for data_source
        builder.HasCheckConstraint(
            "CK_vehicle_data_source",
            "data_source IN ('Manual', 'EasyCars', 'Import')");
    }
}
