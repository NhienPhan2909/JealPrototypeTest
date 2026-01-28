using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JealPrototype.Domain.Entities;

namespace JealPrototype.Infrastructure.Persistence.Configurations;

public class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.ToTable("blog");
        
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id");
        
        builder.Property(b => b.DealershipId).HasColumnName("dealership_id").IsRequired();
        builder.Property(b => b.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
        builder.Property(b => b.Content).HasColumnName("content").IsRequired();
        builder.Property(b => b.ImageUrl).HasColumnName("image_url");
        builder.Property(b => b.PublishedDate).HasColumnName("published_date");
        builder.Property(b => b.Author).HasColumnName("author").IsRequired().HasMaxLength(255);
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        
        builder.Ignore(b => b.UpdatedAt);
    }
}
