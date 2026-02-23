using System.Security.Claims;

namespace Mind_Manager;

public interface IUserLoggedHandller
{
    (Guid? userId, string? userRole) GetUserIdAndRole(ClaimsPrincipal user);
}

public class UserLoggedHandller : IUserLoggedHandller
{
    public (Guid? userId, string? userRole) GetUserIdAndRole(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        return (userId, userRole);
    }
}
