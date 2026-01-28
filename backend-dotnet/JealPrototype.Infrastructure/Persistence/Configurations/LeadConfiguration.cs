using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("lead");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");

        builder.Property(l => l.DealershipId)
            .IsRequired()
            .HasColumnName("dealership_id");

        builder.Property(l => l.VehicleId)
            .HasColumnName("vehicle_id");

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(l => l.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(l => l.Phone)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("phone");

        builder.Property(l => l.Message)
            .IsRequired()
            .HasColumnName("message");

        builder.Property(l => l.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("status")
            .HasConversion(
                s => s == LeadStatus.InProgress ? "in progress" : s.ToString().ToLower(),
                s => s == "received" ? LeadStatus.Received 
                    : s == "in progress" ? LeadStatus.InProgress 
                    : LeadStatus.Done);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Ignore(l => l.UpdatedAt);

        builder.HasIndex(l => l.DealershipId)
            .HasDatabaseName("idx_lead_dealership_id");

        builder.HasIndex(l => l.CreatedAt)
            .HasDatabaseName("idx_lead_created_at")
            .IsDescending();

        builder.HasOne(l => l.Vehicle)
            .WithMany()
            .HasForeignKey(l => l.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
