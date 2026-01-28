using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("blog_post");

        builder.HasKey(bp => bp.Id);
        builder.Property(bp => bp.Id).HasColumnName("id");

        builder.Property(bp => bp.DealershipId)
            .IsRequired()
            .HasColumnName("dealership_id");

        builder.Property(bp => bp.Title)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("title");

        builder.Property(bp => bp.Slug)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("slug");

        builder.Property(bp => bp.Content)
            .IsRequired()
            .HasColumnName("content");

        builder.Property(bp => bp.Excerpt)
            .HasColumnName("excerpt");

        builder.Property(bp => bp.FeaturedImageUrl)
            .HasColumnName("featured_image_url");

        builder.Property(bp => bp.AuthorName)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("author_name");

        builder.Property(bp => bp.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("status")
            .HasConversion(
                s => s.ToString().ToLower(),
                s => Enum.Parse<BlogPostStatus>(s, true));

        builder.Property(bp => bp.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(bp => bp.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(bp => bp.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(bp => bp.DealershipId)
            .HasDatabaseName("idx_blog_post_dealership_id");

        builder.HasIndex(bp => bp.Slug)
            .HasDatabaseName("idx_blog_post_slug");

        builder.HasIndex(bp => new { bp.DealershipId, bp.Slug })
            .IsUnique();
    }
}
