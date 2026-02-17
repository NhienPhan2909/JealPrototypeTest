using FluentAssertions;
using JealPrototype.API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace JealPrototype.Tests.Unit.Filters;

public class RequireDealershipAccessAttributeTests
{
    private ActionExecutingContext CreateContext(
        ClaimsPrincipal user,
        RouteValueDictionary? routeValues = null,
        Dictionary<string, object>? actionArguments = null,
        IQueryCollection? query = null)
    {
        var httpContext = new DefaultHttpContext
        {
            User = user
        };

        if (query != null)
        {
            httpContext.Request.QueryString = new QueryString("?" + 
                string.Join("&", query.Select(kv => $"{kv.Key}={kv.Value}")));
        }

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(routeValues ?? new RouteValueDictionary()),
            new ActionDescriptor()
        );

        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArguments ?? new Dictionary<string, object>(),
            Mock.Of<Controller>()
        );

        var mockLogger = new Mock<ILogger<RequireDealershipAccessAttribute>>();
        httpContext.RequestServices = CreateServiceProvider(mockLogger.Object);

        return actionExecutingContext;
    }

    private IServiceProvider CreateServiceProvider(ILogger<RequireDealershipAccessAttribute> logger)
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(x => x.GetService(typeof(ILogger<RequireDealershipAccessAttribute>)))
            .Returns(logger);
        return serviceProvider.Object;
    }

    private ClaimsPrincipal CreateUser(int dealershipId, string userType = "Manager", int userId = 1)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("dealership_id", dealershipId.ToString()),
            new Claim("user_type", userType)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    [Fact]
    public async Task OnActionExecutionAsync_UserAccessingOwnDealership_ShouldAllow()
    {
        // Arrange
        var attribute = new RequireDealershipAccessAttribute("dealershipId", DealershipAccessSource.Route);
        var user = CreateUser(dealershipId: 123);
        var context = CreateContext(
            user,
            routeValues: new RouteValueDictionary { { "dealershipId", 123 } }
        );

        var nextCalled = false;
        Task<ActionExecutedContext> Next() 
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), Mock.Of<Controller>()));
        }

        // Act
        await attribute.OnActionExecutionAsync(context, Next);

        // Assert
        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_UserAccessingDifferentDealership_ShouldDeny()
    {
        // Arrange
        var attribute = new RequireDealershipAccessAttribute("dealershipId", DealershipAccessSource.Route);
        var user = CreateUser(dealershipId: 123);
        var context = CreateContext(
            user,
            routeValues: new RouteValueDictionary { { "dealershipId", 456 } }
        );

        var nextCalled = false;
        Task<ActionExecutedContext> Next() 
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), Mock.Of<Controller>()));
        }

        // Act
        await attribute.OnActionExecutionAsync(context, Next);

        // Assert
        nextCalled.Should().BeFalse();
        context.Result.Should().BeOfType<ObjectResult>();
        var result = context.Result as ObjectResult;
        result!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task OnActionExecutionAsync_AdminUser_ShouldBypassTenantCheck()
    {
        // Arrange
        var attribute = new RequireDealershipAccessAttribute("dealershipId", DealershipAccessSource.Route);
        var adminUser = CreateUser(dealershipId: 123, userType: "Admin");
        var context = CreateContext(
            adminUser,
            routeValues: new RouteValueDictionary { { "dealershipId", 456 } }
        );

        var nextCalled = false;
        Task<ActionExecutedContext> Next() 
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), Mock.Of<Controller>()));
        }

        // Act
        await attribute.OnActionExecutionAsync(context, Next);

        // Assert
        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_UnauthenticatedUser_ShouldAllow()
    {
        // Arrange
        var attribute = new RequireDealershipAccessAttribute("dealershipId", DealershipAccessSource.Route);
        var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
        var context = CreateContext(
            unauthenticatedUser,
            routeValues: new RouteValueDictionary { { "dealershipId", 123 } }
        );

        var nextCalled = false;
        Task<ActionExecutedContext> Next() 
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), Mock.Of<Controller>()));
        }

        // Act
        await attribute.OnActionExecutionAsync(context, Next);

        // Assert
        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_QuerySource_ShouldExtractFromQuery()
    {
        // Arrange
        var attribute = new RequireDealershipAccessAttribute("dealershipId", DealershipAccessSource.Query);
        var user = CreateUser(dealershipId: 123);
        
        var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "dealershipId", "123" }
        });
        
        var context = CreateContext(user, query: queryCollection);

        var nextCalled = false;
        Task<ActionExecutedContext> Next() 
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), Mock.Of<Controller>()));
        }

        // Act
        await attribute.OnActionExecutionAsync(context, Next);

        // Assert
        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_MissingDealershipId_ShouldReturnBadRequest()
    {
        // Arrange
        var attribute = new RequireDealershipAccessAttribute("dealershipId", DealershipAccessSource.Route);
        var user = CreateUser(dealershipId: 123);
        var context = CreateContext(user, routeValues: new RouteValueDictionary());

        var nextCalled = false;
        Task<ActionExecutedContext> Next() 
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), Mock.Of<Controller>()));
        }

        // Act
        await attribute.OnActionExecutionAsync(context, Next);

        // Assert
        nextCalled.Should().BeFalse();
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task OnActionExecutionAsync_UserWithoutDealershipClaim_ShouldDeny()
    {
        // Arrange
        var attribute = new RequireDealershipAccessAttribute("dealershipId", DealershipAccessSource.Route);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("user_type", "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        
        var context = CreateContext(
            user,
            routeValues: new RouteValueDictionary { { "dealershipId", 123 } }
        );

        var nextCalled = false;
        Task<ActionExecutedContext> Next() 
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), Mock.Of<Controller>()));
        }

        // Act
        await attribute.OnActionExecutionAsync(context, Next);

        // Assert
        nextCalled.Should().BeFalse();
        context.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task OnActionExecutionAsync_RequireAuthenticationTrue_UnauthenticatedUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var attribute = new RequireDealershipAccessAttribute("dealershipId", DealershipAccessSource.Route)
        {
            RequireAuthentication = true
        };
        
        var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
        var context = CreateContext(
            unauthenticatedUser,
            routeValues: new RouteValueDictionary { { "dealershipId", 123 } }
        );

        var nextCalled = false;
        Task<ActionExecutedContext> Next() 
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), Mock.Of<Controller>()));
        }

        // Act
        await attribute.OnActionExecutionAsync(context, Next);

        // Assert
        nextCalled.Should().BeFalse();
        context.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
