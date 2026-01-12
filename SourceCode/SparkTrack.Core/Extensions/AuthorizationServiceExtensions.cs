namespace SparkTrack.Core.Extensions;

using Exceptions;
using Services.Authorization;
using Shared.Data.Entities;
using Shared.Enums;

public static class AuthorizationServiceExtensions
{
    public static User GetUserOrThrowIfUnauthorized(this IAuthorizationService service)
    {
        if (service.CurrentUser is null)
            throw new UnauthorizedException(
                $"Authorize user using {nameof(IAuthorizationService)}.{nameof(IAuthorizationService.AuthorizeAsync)}"
            );

        return service.CurrentUser;
    }

    public static User GetUserOrThrowIfNotInRole(
        this IAuthorizationService service,
        ERole role,
        params ERole[] otherRoles
    )
    {
        if (service.CurrentUser is null)
            throw new UnauthorizedException(
                $"Authorize user using {nameof(IAuthorizationService)}.{nameof(IAuthorizationService.AuthorizeAsync)}"
            );

        if (service.CurrentUser.Role != role)
            throw new ForbiddenException(
                $"Authorize user with required role ({role}) using {nameof(IAuthorizationService)}.{nameof(IAuthorizationService.AuthorizeAsync)}"
            );

        return service.CurrentUser;
    }
}