using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class BlogRepository : IBlogRepository
{
    private readonly ApplicationDbContext _context;

    public BlogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Blog?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Blogs.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Blog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Blogs.ToListAsync(cancellationToken);
    }

    public async Task<Blog> AddAsync(Blog entity, CancellationToken cancellationToken = default)
    {
        await _context.Blogs.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Blog entity, CancellationToken cancellationToken = default)
    {
        _context.Blogs.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Blog entity, CancellationToken cancellationToken = default)
    {
        _context.Blogs.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Blog entity)
    {
        _context.Blogs.Update(entity);
    }

    public void Delete(Blog entity)
    {
        _context.Blogs.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Blog>> GetByDealershipIdAsync(int dealershipId)
    {
        return await _context.Blogs
            .Where(b => b.DealershipId == dealershipId)
            .OrderByDescending(b => b.PublishedDate)
            .ToListAsync();
    }
}
