using System.Security.Claims;

namespace Cheetah.Permissions;

/// <summary>
/// Проверка прав по ClaimsPrincipal. Реализация — stateless, без I/O: смотрит только
/// на claims в принципале. Claims приходят из JWT, который Identity подкладывает
/// при логине (IdentityRoleClaim'ы пользовательских ролей + IdentityUserClaim'ы).
///
/// Если нужна более сложная логика (проверка scope, иерархия permissions) —
/// реализуйте через собственную реализацию IPermissionAuthorizer.
/// </summary>
public interface IPermissionAuthorizer
{
    /// <summary>Истинно, если у пользователя есть указанный permission.</summary>
    bool Has(ClaimsPrincipal user, string permission);

    /// <summary>Истинно, если у пользователя есть ВСЕ указанные permissions.</summary>
    bool HasAll(ClaimsPrincipal user, IEnumerable<string> permissions);

    /// <summary>Истинно, если у пользователя есть ХОТЯ БЫ ОДИН из указанных permissions.</summary>
    bool HasAny(ClaimsPrincipal user, IEnumerable<string> permissions);
}

public sealed class ClaimPermissionAuthorizer : IPermissionAuthorizer
{
    public bool Has(ClaimsPrincipal user, string permission)
    {
        if (user.Identity?.IsAuthenticated != true) return false;
        return user.HasClaim(PermissionConstants.PermissionClaimType, permission);
    }

    public bool HasAll(ClaimsPrincipal user, IEnumerable<string> permissions)
    {
        if (user.Identity?.IsAuthenticated != true) return false;
        return permissions.All(p => user.HasClaim(PermissionConstants.PermissionClaimType, p));
    }

    public bool HasAny(ClaimsPrincipal user, IEnumerable<string> permissions)
    {
        if (user.Identity?.IsAuthenticated != true) return false;
        return permissions.Any(p => user.HasClaim(PermissionConstants.PermissionClaimType, p));
    }
}
