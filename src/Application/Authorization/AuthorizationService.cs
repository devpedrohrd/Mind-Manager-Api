using Mind_Manager.Domain.Entities;
using Mind_Manager.Domain.Exceptions;

namespace Mind_Manager.Application.Authorization;

public interface IAuthorizationService
{
    void ValidateResourceOwnership(Guid resourceOwnerId, Guid requestingUserId, UserRole requestingUserRole);
    bool CanModifyUser(Guid targetUserId, Guid requestingUserId, UserRole requestingUserRole);
}

public class AuthorizationService : IAuthorizationService
{
    public void ValidateResourceOwnership(Guid resourceOwnerId, Guid requestingUserId, UserRole requestingUserRole)
    {
        if (!CanModifyUser(resourceOwnerId, requestingUserId, requestingUserRole))
            throw new ForbiddenException("You don't have permission to access this resource");
    }

    public bool CanModifyUser(Guid targetUserId, Guid requestingUserId, UserRole requestingUserRole)
    {
        return requestingUserRole == UserRole.Admin || targetUserId == requestingUserId;
    }
}
