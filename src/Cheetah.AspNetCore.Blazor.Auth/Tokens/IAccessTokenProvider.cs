namespace Cheetah.AspNetCore.Blazor.Auth.Tokens;

/// <summary>
/// Достаёт access-токен текущего пользователя для исходящих вызовов к микросервисам.
/// Безопасен внутри Blazor-контура: текущая сессия берётся из <see cref="System.Security.Claims.ClaimsPrincipal"/>
/// через AuthenticationStateProvider, токены — из <see cref="IUserTokenStore"/> (без HttpContext).
/// </summary>
public interface IAccessTokenProvider
{
    /// <summary>
    /// Возвращает действующий access-токен (при необходимости обновляя протухший по refresh),
    /// либо <c>null</c>, если пользователь не аутентифицирован или токен недоступен/обновить нельзя.
    /// </summary>
    Task<string?> GetAccessTokenAsync(CancellationToken ct = default);

    /// <summary>
    /// Принудительно обновляет токен через refresh (используется при 401 от микросервиса).
    /// Возвращает новый access-токен или <c>null</c>, если обновление невозможно.
    /// </summary>
    Task<string?> RefreshAccessTokenAsync(CancellationToken ct = default);
}
