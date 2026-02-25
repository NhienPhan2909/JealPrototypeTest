using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class EasyCarsCredentialConfiguration : IEntityTypeConfiguration<EasyCarsCredential>
{
    public void Configure(EntityTypeBuilder<EasyCarsCredential> builder)
    {
        builder.ToTable("dealership_easycars_credentials");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.DealershipId)
            .IsRequired()
            .HasColumnName("dealership_id");

        builder.Property(c => c.AccountNumberEncrypted)
            .IsRequired()
            .HasColumnName("account_number_encrypted");

        builder.Property(c => c.AccountSecretEncrypted)
            .IsRequired()
            .HasColumnName("account_secret_encrypted");

        builder.Property(c => c.EncryptionIV)
            .IsRequired()
            .HasColumnName("encryption_iv");

        builder.Property(c => c.Environment)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("environment");

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.YardCode)
            .HasMaxLength(50)
            .HasColumnName("yard_code");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.LastSyncedAt)
            .HasColumnName("last_synced_at");

        // UNIQUE constraint on dealership_id
        builder.HasIndex(c => c.DealershipId)
            .HasDatabaseName("idx_easycars_credentials_dealership")
            .IsUnique();

        // Partial index on is_active
        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("idx_easycars_credentials_active")
            .HasFilter("is_active = true");

        // CHECK constraint for environment
        builder.HasCheckConstraint(
            "CK_easycars_credentials_environment",
            "environment IN ('Test', 'Production')");

        // FK relationship to dealerships
        builder.HasOne(c => c.Dealership)
            .WithMany()
            .HasForeignKey(c => c.DealershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
