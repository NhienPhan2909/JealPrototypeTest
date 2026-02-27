using System.Security.Claims;

namespace JealPrototype.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetDealershipId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("dealershipid")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(claim, out var id))
            return id;
        
        throw new UnauthorizedAccessException("Invalid user ID claim");
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.FindFirst("usertype")?.Value == "Admin";
    }

    public static bool IsDealershipOwner(this ClaimsPrincipal user)
    {
        return user.FindFirst("usertype")?.Value == "DealershipOwner";
    }

    public static string GetUserType(this ClaimsPrincipal user)
    {
        return user.FindFirst("usertype")?.Value ?? string.Empty;
    }

    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        if (IsAdmin(user) || IsDealershipOwner(user))
            return true;

        return user.Claims.Any(c => c.Type == "permission" && c.Value == permission);
    }
}
