using JealPrototype.API.Extensions;
using JealPrototype.API.Filters;
using JealPrototype.API.Middleware;
using JealPrototype.Application.UseCases.Auth;
using JealPrototype.Application.UseCases.BlogPost;
using JealPrototype.Application.UseCases.Dealership;
using JealPrototype.Application.UseCases.DesignTemplates;
using JealPrototype.Application.UseCases.Lead;
using JealPrototype.Application.UseCases.SalesRequest;
using JealPrototype.Application.UseCases.User;
using JealPrototype.Application.UseCases.Vehicle;
using JealPrototype.Domain.Interfaces;
using AspNetCoreRateLimit;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// Add environment variable support
builder.Configuration.AddEnvironmentVariables();

// Configure rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Add CORS
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() 
        ?? new[] { "http://localhost:3000" };
    
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add infrastructure services (DbContext, Repositories, Services)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add application services (AutoMapper, FluentValidation)
builder.Services.AddApplicationServices();

// Add JWT authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// Add Hangfire
builder.Services.AddHangfire(config => config
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")))
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings());

builder.Services.AddHangfireServer();

// Register all Use Cases
// Auth
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<GetCurrentUserUseCase>();

// Background Jobs
builder.Services.AddScoped<JealPrototype.Application.BackgroundJobs.StockSyncBackgroundJob>();

// Dealership
builder.Services.AddScoped<CreateDealershipUseCase>();
builder.Services.AddScoped<GetDealershipUseCase>();
builder.Services.AddScoped<GetAllDealershipsUseCase>();
builder.Services.AddScoped<GetDealershipByUrlUseCase>();
builder.Services.AddScoped<UpdateDealershipUseCase>();
builder.Services.AddScoped<DeleteDealershipUseCase>();

// Vehicle
builder.Services.AddScoped<CreateVehicleUseCase>();
builder.Services.AddScoped<GetVehiclesUseCase>();
builder.Services.AddScoped<GetVehicleByIdUseCase>();
builder.Services.AddScoped<GetDealershipVehiclesUseCase>();
builder.Services.AddScoped<UpdateVehicleUseCase>();
builder.Services.AddScoped<DeleteVehicleUseCase>();

// User
builder.Services.AddScoped<CreateUserUseCase>();
builder.Services.AddScoped<GetUsersUseCase>();
builder.Services.AddScoped<GetUserByIdUseCase>();
builder.Services.AddScoped<GetDealershipUsersUseCase>();
builder.Services.AddScoped<UpdateUserUseCase>();
builder.Services.AddScoped<DeleteUserUseCase>();

// Lead
builder.Services.AddScoped<CreateLeadUseCase>();
builder.Services.AddScoped<GetLeadsUseCase>();
builder.Services.AddScoped<UpdateLeadStatusUseCase>();
builder.Services.AddScoped<DeleteLeadUseCase>();

// SalesRequest
builder.Services.AddScoped<CreateSalesRequestUseCase>();
builder.Services.AddScoped<GetSalesRequestsUseCase>();
builder.Services.AddScoped<UpdateSalesRequestStatusUseCase>();
builder.Services.AddScoped<DeleteSalesRequestUseCase>();

// BlogPost
builder.Services.AddScoped<CreateBlogPostUseCase>();
builder.Services.AddScoped<GetBlogPostsUseCase>();
builder.Services.AddScoped<GetBlogPostByIdUseCase>();
builder.Services.AddScoped<GetBlogPostBySlugUseCase>();
builder.Services.AddScoped<UpdateBlogPostUseCase>();
builder.Services.AddScoped<DeleteBlogPostUseCase>();

// DesignTemplate
builder.Services.AddScoped<GetDesignTemplatesUseCase>();
builder.Services.AddScoped<CreateDesignTemplateUseCase>();
builder.Services.AddScoped<DeleteDesignTemplateUseCase>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Jeal Prototype API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Rate limiting middleware
app.UseIpRateLimiting();

// CORS
app.UseCors("AllowFrontend");

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Register Hangfire recurring jobs using service-based API (storage is ready after middleware pipeline)
using (var scope = app.Services.CreateScope())
{
    var systemSettingsRepo = scope.ServiceProvider.GetRequiredService<ISystemSettingsRepository>();
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var cronExpression = await systemSettingsRepo.GetValueAsync("easycar_sync_cron") ?? "0 2 * * *";

    recurringJobManager.AddOrUpdate<JealPrototype.Application.BackgroundJobs.StockSyncBackgroundJob>(
        "easycar-stock-sync",
        job => job.ExecuteAsync(CancellationToken.None),
        cronExpression,
        new Hangfire.RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
}

app.Run();

// Make Program accessible to integration tests
public partial class Program { }

