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
///         await perms.RequireAsync(ContractPermissions.Sign);
///         // ...
///     }
/// }
/// </code>
///
/// Методы асинхронны, потому что permission может быть привязан к фич-флагу, а движок флагов
/// асинхронен: выключенная фича отзывает право, даже если claim выдан.
///
/// За пределами HTTP-запроса (background-сервис, фоновое задание) пользователя нет —
/// все методы возвращают <c>false</c>. Для тех мест используйте <see cref="IPermissionAuthorizer"/>
/// напрямую с явным principal.
/// </summary>
public interface ICurrentUserPermissions
{
    /// <summary>Истинно, если у текущего пользователя есть указанный permission.</summary>
    ValueTask<bool> HasAsync(string permission, CancellationToken ct = default);

    /// <summary>Истинно, если у текущего пользователя есть ВСЕ указанные permissions.</summary>
    ValueTask<bool> HasAllAsync(IEnumerable<string> permissions, CancellationToken ct = default);

    /// <summary>Истинно, если у текущего пользователя есть ХОТЯ БЫ ОДИН из указанных.</summary>
    ValueTask<bool> HasAnyAsync(IEnumerable<string> permissions, CancellationToken ct = default);

    /// <summary>
    /// Бросает <see cref="UnauthorizedAccessException"/>, если у текущего пользователя
    /// нет указанного permission. Удобно использовать как guard в начале метода.
    /// </summary>
    ValueTask RequireAsync(string permission, CancellationToken ct = default);
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

    public ValueTask<bool> HasAsync(string permission, CancellationToken ct = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user is null ? ValueTask.FromResult(false) : _authorizer.HasAsync(user, permission, ct);
    }

    public ValueTask<bool> HasAllAsync(IEnumerable<string> permissions, CancellationToken ct = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user is null ? ValueTask.FromResult(false) : _authorizer.HasAllAsync(user, permissions, ct);
    }

    public ValueTask<bool> HasAnyAsync(IEnumerable<string> permissions, CancellationToken ct = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user is null ? ValueTask.FromResult(false) : _authorizer.HasAnyAsync(user, permissions, ct);
    }

    public async ValueTask RequireAsync(string permission, CancellationToken ct = default)
    {
        if (!await HasAsync(permission, ct))
            throw new UnauthorizedAccessException($"Permission required: {permission}");
    }
}
