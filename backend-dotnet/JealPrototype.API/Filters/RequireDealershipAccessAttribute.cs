using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Reflection;

namespace JealPrototype.API.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequireDealershipAccessAttribute : Attribute, IAsyncActionFilter
{
    public string ParameterName { get; }
    public DealershipAccessSource Source { get; }
    public bool AllowAdmin { get; set; } = true;
    public bool RequireAuthentication { get; set; } = false;

    public RequireDealershipAccessAttribute(
        string parameterName,
        DealershipAccessSource source = DealershipAccessSource.Route)
    {
        ParameterName = parameterName;
        Source = source;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        var isAuthenticated = user.Identity?.IsAuthenticated ?? false;

        // If authentication is required and user is not authenticated
        if (RequireAuthentication && !isAuthenticated)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                success = false,
                message = "Authentication required",
                error = "No valid JWT token provided"
            });
            return;
        }

        // If user is not authenticated and authentication is optional, allow public access
        if (!isAuthenticated)
        {
            await next();
            return;
        }

        // User is authenticated - validate tenant isolation
        var userType = user.FindFirst("usertype")?.Value;
        if (AllowAdmin && userType == "Admin")
        {
            await next();
            return;
        }

        var userDealershipIdClaim = user.FindFirst("dealershipid")?.Value;
        if (string.IsNullOrEmpty(userDealershipIdClaim) || !int.TryParse(userDealershipIdClaim, out int userDealershipId))
        {
            // Authenticated user without dealership_id (not Admin) - deny access
            context.Result = new ForbidResult();
            return;
        }

        int? requestedDealershipId = ExtractDealershipId(context);

        if (requestedDealershipId == null)
        {
            context.Result = new BadRequestObjectResult(new
            {
                success = false,
                message = "Invalid request",
                error = $"Parameter '{ParameterName}' is required"
            });
            return;
        }

        if (userDealershipId != requestedDealershipId.Value)
        {
            var logger = context.HttpContext.RequestServices
                .GetService<ILogger<RequireDealershipAccessAttribute>>();
            
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var endpoint = context.HttpContext.Request.Path;
            
            logger?.LogWarning(
                "Tenant isolation violation attempt: User {UserId} (Dealership {UserDealershipId}) " +
                "attempted to access Dealership {RequestedDealershipId} at {Endpoint}",
                userId, userDealershipId, requestedDealershipId.Value, endpoint);

            context.Result = new ObjectResult(new
            {
                success = false,
                message = "Access denied",
                error = $"You do not have permission to access dealership {requestedDealershipId.Value}"
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }

    private int? ExtractDealershipId(ActionExecutingContext context)
    {
        string? value = null;

        switch (Source)
        {
            case DealershipAccessSource.Route:
                if (context.RouteData.Values.TryGetValue(ParameterName, out var routeValue))
                    value = routeValue?.ToString();
                break;

            case DealershipAccessSource.Query:
                value = context.HttpContext.Request.Query[ParameterName].FirstOrDefault();
                break;

            case DealershipAccessSource.Body:
                // Extract from action arguments (already model-bound)
                foreach (var arg in context.ActionArguments)
                {
                    if (arg.Value == null) continue;
                    
                    var property = arg.Value.GetType().GetProperty(ParameterName, 
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    
                    if (property != null)
                    {
                        var propValue = property.GetValue(arg.Value);
                        if (propValue != null)
                        {
                            value = propValue.ToString();
                            break;
                        }
                    }
                }
                break;
        }

        return int.TryParse(value, out int result) ? result : null;
    }
}

public enum DealershipAccessSource
{
    Route,
    Query,
    Body
}
