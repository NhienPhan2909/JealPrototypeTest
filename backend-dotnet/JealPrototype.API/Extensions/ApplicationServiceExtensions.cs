using System.Reflection;
using FluentValidation;
using JealPrototype.Application.Mappings;
using JealPrototype.Application.UseCases.EasyCars;

namespace JealPrototype.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        
        services.AddValidatorsFromAssembly(Assembly.Load("JealPrototype.Application"));
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("JealPrototype.Application")));
        
        // Register EasyCars use cases
        services.AddScoped<CreateCredentialUseCase>();
        services.AddScoped<GetCredentialUseCase>();
        services.AddScoped<UpdateCredentialUseCase>();
        services.AddScoped<DeleteCredentialUseCase>();
        services.AddScoped<TestConnectionUseCase>();
        
        return services;
    }
}
