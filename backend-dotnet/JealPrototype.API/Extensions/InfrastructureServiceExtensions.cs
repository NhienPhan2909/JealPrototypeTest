using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Infrastructure.Persistence;
using JealPrototype.Infrastructure.Persistence.Repositories;
using JealPrototype.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JealPrototype.API.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is not configured");

        // Configure Npgsql data source with JSON support
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(dataSource));

        services.AddScoped<IDealershipRepository, DealershipRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<ISalesRequestRepository, SalesRequestRepository>();
        services.AddScoped<IBlogPostRepository, BlogPostRepository>();
        services.AddScoped<IDesignTemplateRepository, DesignTemplateRepository>();
        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IHeroMediaRepository, HeroMediaRepository>();
        services.AddScoped<IPromotionalPanelRepository, PromotionalPanelRepository>();

        services.AddScoped<IAuthService, JwtAuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IGoogleReviewsService, GoogleReviewsService>();
        services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();

        return services;
    }
}
