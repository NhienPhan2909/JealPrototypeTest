using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using System.Reflection;

namespace JealPrototype.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Dealership> Dealerships => Set<Dealership>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<SalesRequest> SalesRequests => Set<SalesRequest>();
    public DbSet<User> Users => Set<User>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<DesignTemplate> DesignTemplates => Set<DesignTemplate>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<HeroMedia> HeroMedia => Set<HeroMedia>();
    public DbSet<PromotionalPanel> PromotionalPanels => Set<PromotionalPanel>();
    public DbSet<EasyCarsCredential> EasyCarsCredentials => Set<EasyCarsCredential>();
    public DbSet<EasyCarsSyncLog> EasyCarsSyncLogs => Set<EasyCarsSyncLog>();
    public DbSet<EasyCarsStockData> EasyCarsStockData => Set<EasyCarsStockData>();
    public DbSet<DealershipSettings> DealershipSettings => Set<DealershipSettings>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
    public DbSet<LeadStatusConflict> LeadStatusConflicts => Set<LeadStatusConflict>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
    }
}

public class UtcDateTimeConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
