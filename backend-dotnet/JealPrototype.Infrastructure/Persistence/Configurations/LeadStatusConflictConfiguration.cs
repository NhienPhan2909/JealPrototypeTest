using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class LeadStatusConflictConfiguration : IEntityTypeConfiguration<LeadStatusConflict>
{
    public void Configure(EntityTypeBuilder<LeadStatusConflict> builder)
    {
        builder.ToTable("lead_status_conflicts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.DealershipId).IsRequired().HasColumnName("dealership_id");
        builder.Property(c => c.LeadId).IsRequired().HasColumnName("lead_id");
        builder.Property(c => c.EasyCarsLeadNumber).IsRequired().HasMaxLength(100).HasColumnName("easycars_lead_number");
        builder.Property(c => c.LocalStatus).IsRequired().HasMaxLength(20).HasColumnName("local_status");
        builder.Property(c => c.RemoteStatus).IsRequired().HasColumnName("remote_status");
        builder.Property(c => c.DetectedAt).IsRequired().HasColumnName("detected_at");
        builder.Property(c => c.IsResolved).IsRequired().HasDefaultValue(false).HasColumnName("is_resolved");
        builder.Property(c => c.ResolvedAt).HasColumnName("resolved_at");
        builder.Property(c => c.ResolvedBy).HasMaxLength(255).HasColumnName("resolved_by");
        builder.Property(c => c.Resolution).HasMaxLength(10).HasColumnName("resolution");

        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.Ignore(c => c.UpdatedAt);

        builder.HasIndex(c => c.DealershipId).HasDatabaseName("idx_lead_status_conflicts_dealership");
        builder.HasIndex(c => c.LeadId).HasDatabaseName("idx_lead_status_conflicts_lead");
        builder.HasIndex(c => new { c.DealershipId, c.IsResolved }).HasDatabaseName("idx_lead_status_conflicts_dealership_unresolved");
    }
}
