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
    }
}
