using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class BlogPostRepository : IBlogPostRepository
{
    private readonly ApplicationDbContext _context;

    public BlogPostRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BlogPost?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.BlogPosts.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<BlogPost>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BlogPosts.ToListAsync(cancellationToken);
    }

    public async Task<BlogPost> AddAsync(BlogPost entity, CancellationToken cancellationToken = default)
    {
        await _context.BlogPosts.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(BlogPost entity, CancellationToken cancellationToken = default)
    {
        _context.BlogPosts.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(BlogPost entity, CancellationToken cancellationToken = default)
    {
        _context.BlogPosts.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(BlogPost entity)
    {
        _context.BlogPosts.Update(entity);
    }

    public void Delete(BlogPost entity)
    {
        _context.BlogPosts.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<BlogPost>> GetByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.BlogPosts
            .Where(bp => bp.DealershipId == dealershipId)
            .OrderByDescending(bp => bp.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BlogPost>> GetPublishedByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.BlogPosts
            .Where(bp => bp.DealershipId == dealershipId && bp.Status == BlogPostStatus.Published)
            .OrderByDescending(bp => bp.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<BlogPost?> GetBySlugAsync(string slug, int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.BlogPosts
            .FirstOrDefaultAsync(bp => bp.Slug == slug && bp.DealershipId == dealershipId, cancellationToken);
    }

    public async Task<BlogPost?> GetByIdAndDealershipAsync(int id, int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.BlogPosts
            .FirstOrDefaultAsync(bp => bp.Id == id && bp.DealershipId == dealershipId, cancellationToken);
    }
}
