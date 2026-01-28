using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class SalesRequestConfiguration : IEntityTypeConfiguration<SalesRequest>
{
    public void Configure(EntityTypeBuilder<SalesRequest> builder)
    {
        builder.ToTable("sales_request");

        builder.HasKey(sr => sr.Id);
        builder.Property(sr => sr.Id).HasColumnName("id");

        builder.Property(sr => sr.DealershipId)
            .IsRequired()
            .HasColumnName("dealership_id");

        builder.Property(sr => sr.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("name");

        builder.Property(sr => sr.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(sr => sr.Phone)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("phone");

        builder.Property(sr => sr.Make)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("make");

        builder.Property(sr => sr.Model)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("model");

        builder.Property(sr => sr.Year)
            .IsRequired()
            .HasColumnName("year");

        builder.Property(sr => sr.Kilometers)
            .IsRequired()
            .HasColumnName("kilometers");

        builder.Property(sr => sr.AdditionalMessage)
            .HasColumnName("additional_message");

        builder.Property(sr => sr.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("status")
            .HasConversion(
                s => s == LeadStatus.InProgress ? "in progress" : s.ToString().ToLower(),
                s => s == "received" ? LeadStatus.Received 
                    : s == "in progress" ? LeadStatus.InProgress 
                    : LeadStatus.Done);

        builder.Property(sr => sr.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Ignore(sr => sr.UpdatedAt);

        builder.HasIndex(sr => sr.DealershipId)
            .HasDatabaseName("idx_sales_request_dealership_id");

        builder.HasIndex(sr => sr.CreatedAt)
            .HasDatabaseName("idx_sales_request_created_at")
            .IsDescending();
    }
}
