using JealPrototype.Application.BackgroundJobs;
using JealPrototype.Application.Interfaces;
using JealPrototype.Application.Interfaces.Security;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Infrastructure.Configuration;
using JealPrototype.Infrastructure.ExternalServices;
using JealPrototype.Infrastructure.Persistence;
using JealPrototype.Infrastructure.Persistence.Repositories;
using JealPrototype.Infrastructure.Security;
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

        // Configure encryption settings
        services.Configure<EncryptionSettings>(options =>
        {
            var encryptionKey = Environment.GetEnvironmentVariable("EASYCARS_ENCRYPTION_KEY") 
                ?? configuration.GetSection(EncryptionSettings.SectionName)["EncryptionKey"];
            options.EncryptionKey = encryptionKey ?? string.Empty;
            options.KeyVersion = configuration.GetSection(EncryptionSettings.SectionName).GetValue<int>("KeyVersion", 1);
        });

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
        services.AddScoped<IEasyCarsCredentialRepository, EasyCarsCredentialRepository>();
        services.AddScoped<IEasyCarsStockDataRepository, EasyCarsStockDataRepository>();
        services.AddScoped<IEasyCarsSyncLogRepository, EasyCarsSyncLogRepository>();
        services.AddScoped<IDealershipSettingsRepository, DealershipSettingsRepository>();
        services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();
        services.AddScoped<ILeadStatusConflictRepository, LeadStatusConflictRepository>();

        services.AddScoped<IAuthService, JwtAuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IGoogleReviewsService, GoogleReviewsService>();
        services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        
        // Register encryption service for credentials (Story 1.2)
        // Note: IEncryptionService already provides this functionality from Story 1.1
        // Registering ICredentialEncryptionService as alias for backward compatibility
        services.AddScoped<ICredentialEncryptionService, CredentialEncryptionService>();

        // Register EasyCars configuration (Story 1.6)
        services.Configure<JealPrototype.Infrastructure.Configuration.EasyCarsConfiguration>(
            configuration.GetSection(JealPrototype.Infrastructure.Configuration.EasyCarsConfiguration.SectionName));

        // Register memory cache for token management (Story 1.6)
        services.AddMemoryCache();

        // Register EasyCars API client with updated timeout (Story 1.6)
        services.AddHttpClient("EasyCarsApi", client =>
        {
            var timeout = configuration.GetValue<int>("EasyCars:TimeoutSeconds", 30);
            client.Timeout = TimeSpan.FromSeconds(timeout);
        });
        services.AddScoped<IEasyCarsApiClient, EasyCarsApiClient>();

        // Register EasyCars stock mapper (Story 2.2)
        services.AddScoped<IEasyCarsStockMapper, JealPrototype.Application.Services.EasyCars.EasyCarsStockMapper>();

        // Register EasyCars lead mapper (Story 3.2)
        services.AddScoped<IEasyCarsLeadMapper, JealPrototype.Application.Services.EasyCars.EasyCarsLeadMapper>();

        // Register EasyCars stock sync service (Story 2.3)
        services.AddScoped<IEasyCarsStockSyncService, JealPrototype.Application.Services.EasyCars.EasyCarsStockSyncService>();

        // Register EasyCars lead sync service (Story 3.3)
        services.AddScoped<IEasyCarsLeadSyncService, JealPrototype.Application.Services.EasyCars.EasyCarsLeadSyncService>();

        // Register EasyCars lead sync background job (Story 3.3)
        services.AddScoped<LeadSyncBackgroundJob>();

        // Register image download service (Story 2.6)
        services.AddHttpClient("ImageDownload")
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        services.AddScoped<IImageDownloadService, JealPrototype.Application.Services.EasyCars.ImageDownloadService>();

        return services;
    }
}
