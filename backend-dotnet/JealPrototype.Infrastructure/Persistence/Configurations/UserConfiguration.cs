using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.ValueObjects;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("app_user");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("username");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasColumnName("password_hash");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("email")
            .HasConversion(
                email => email.Value,
                value => Email.Create(value));

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("full_name");

        builder.Property(u => u.UserType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("user_type")
            .HasConversion(
                ut => ut == UserType.Admin ? "admin" 
                    : ut == UserType.DealershipOwner ? "dealership_owner" 
                    : "dealership_staff",
                ut => ut == "admin" ? UserType.Admin 
                    : ut == "dealership_owner" ? UserType.DealershipOwner 
                    : UserType.DealershipStaff);

        builder.Property(u => u.DealershipId)
            .HasColumnName("dealership_id");

        builder.Property(u => u.Permissions)
            .HasColumnName("permissions")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .HasConversion(
                p => System.Text.Json.JsonSerializer.Serialize(p.Select(x => x.ToString().ToLower()), (System.Text.Json.JsonSerializerOptions?)null),
                p => System.Text.Json.JsonSerializer.Deserialize<List<string>>(p, (System.Text.Json.JsonSerializerOptions?)null)!
                    .Select(x => Enum.Parse<Permission>(x, true)).ToList());

        builder.Property(u => u.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Ignore(u => u.UpdatedAt);

        builder.HasIndex(u => u.Username)
            .HasDatabaseName("idx_app_user_username")
            .IsUnique();

        builder.HasIndex(u => u.DealershipId)
            .HasDatabaseName("idx_app_user_dealership_id");
    }
}
