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

        // EasyCars Integration Fields
        builder.Property(l => l.EasyCarsLeadNumber)
            .HasMaxLength(100)
            .HasColumnName("easycars_lead_number");

        builder.Property(l => l.EasyCarsCustomerNo)
            .HasMaxLength(100)
            .HasColumnName("easycars_customer_no");

        builder.Property(l => l.EasyCarsRawData)
            .HasColumnName("easycars_raw_data")
            .HasColumnType("jsonb");

        builder.Property(l => l.DataSource)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("data_source")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<DataSource>(v))
            .HasDefaultValue(DataSource.Manual);

        builder.Property(l => l.LastSyncedToEasyCars)
            .HasColumnName("last_synced_to_easycars");

        builder.Property(l => l.LastSyncedFromEasyCars)
            .HasColumnName("last_synced_from_easycars");

        builder.Property(l => l.VehicleInterestType)
            .HasMaxLength(50)
            .HasColumnName("vehicle_interest_type");

        builder.Property(l => l.FinanceInterested)
            .HasColumnName("finance_interested")
            .HasDefaultValue(false);

        builder.Property(l => l.Rating)
            .HasMaxLength(20)
            .HasColumnName("rating");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Ignore(l => l.UpdatedAt);

        builder.HasIndex(l => l.DealershipId)
            .HasDatabaseName("idx_lead_dealership_id");

        builder.HasIndex(l => l.CreatedAt)
            .HasDatabaseName("idx_lead_created_at")
            .IsDescending();

        // EasyCars indexes
        builder.HasIndex(l => l.EasyCarsLeadNumber)
            .HasDatabaseName("idx_leads_easycars_lead")
            .HasFilter("easycars_lead_number IS NOT NULL");

        builder.HasIndex(l => l.DataSource)
            .HasDatabaseName("idx_leads_data_source");

        // CHECK constraint for data_source
        builder.HasCheckConstraint(
            "CK_lead_data_source",
            "data_source IN ('Manual', 'EasyCars', 'WebForm')");

        builder.HasOne(l => l.Vehicle)
            .WithMany()
            .HasForeignKey(l => l.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
