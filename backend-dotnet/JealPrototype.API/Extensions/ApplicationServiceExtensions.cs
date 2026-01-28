using System.Reflection;
using FluentValidation;
using JealPrototype.Application.Mappings;

namespace JealPrototype.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        
        services.AddValidatorsFromAssembly(Assembly.Load("JealPrototype.Application"));
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("JealPrototype.Application")));
        
        return services;
    }
}
