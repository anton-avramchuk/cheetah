using Microsoft.AspNetCore.Http;

namespace Cheetah.Permissions;

/// <summary>
/// Удобная обёртка над <see cref="IPermissionAuthorizer"/>: берёт текущего пользователя
/// из <see cref="IHttpContextAccessor"/> и проверяет permission'ы без необходимости
/// тянуть <see cref="System.Security.Claims.ClaimsPrincipal"/> вручную.
///
/// Используется в Application-сервисах, Command/Query-хендлерах:
/// <code>
/// public class SignContractHandler(ICurrentUserPermissions perms)
/// {
///     public async Task HandleAsync(...)
///     {
///         if (!perms.Has(ContractPermissions.Sign))
///             throw new ForbiddenException();
///         // ...
///     }
/// }
/// </code>
///
/// За пределами HTTP-запроса (background-сервис, фоновое задание) пользователя нет —
/// все методы возвращают <c>false</c>. Для тех мест используйте <see cref="IPermissionAuthorizer"/>
/// напрямую с явным principal.
/// </summary>
public interface ICurrentUserPermissions
{
    /// <summary>Истинно, если у текущего пользователя есть указанный permission.</summary>
    bool Has(string permission);

    /// <summary>Истинно, если у текущего пользователя есть ВСЕ указанные permissions.</summary>
    bool HasAll(IEnumerable<string> permissions);

    /// <summary>Истинно, если у текущего пользователя есть ХОТЯ БЫ ОДИН из указанных.</summary>
    bool HasAny(IEnumerable<string> permissions);

    /// <summary>
    /// Бросает <see cref="UnauthorizedAccessException"/>, если у текущего пользователя
    /// нет указанного permission. Удобно использовать как guard в начале метода.
    /// </summary>
    void Require(string permission);
}

public sealed class CurrentUserPermissions : ICurrentUserPermissions
{
    private readonly IPermissionAuthorizer _authorizer;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserPermissions(
        IPermissionAuthorizer authorizer,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizer = authorizer;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool Has(string permission)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user is not null && _authorizer.Has(user, permission);
    }

    public bool HasAll(IEnumerable<string> permissions)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user is not null && _authorizer.HasAll(user, permissions);
    }

    public bool HasAny(IEnumerable<string> permissions)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user is not null && _authorizer.HasAny(user, permissions);
    }

    public void Require(string permission)
    {
        if (!Has(permission))
            throw new UnauthorizedAccessException($"Permission required: {permission}");
    }
}
