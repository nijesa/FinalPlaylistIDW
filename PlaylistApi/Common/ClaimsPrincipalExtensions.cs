using System.Security.Claims;

namespace PlaylistApi.Common;

public static class ClaimsPrincipalExtensions
{
    // Lee el id del usuario desde el claim "sub" del JWT.
    // (Equivale al @CurrentUser() + user.userId que usábamos en NestJS.)
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst("sub")?.Value;

        if (!int.TryParse(sub, out var userId))
            throw new HttpException(401, "Unauthorized");

        return userId;
    }
}
