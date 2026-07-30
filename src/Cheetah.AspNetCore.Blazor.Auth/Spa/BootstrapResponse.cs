using System.Security.Claims;
using Cheetah.Core.Security.Claims;

namespace Cheetah.AspNetCore.Blazor.Auth.Spa;

/// <summary>Пользователь текущей сессии для SPA.</summary>
public sealed record BootstrapUser(
    string Id,
    string UserName,
    string? Email,
    IReadOnlyList<string> Roles);

/// <summary>
/// Ответ <c>GET /api/bootstrap</c> — всё, что нужно SPA для гейтинга UI, одним вызовом.
/// </summary>
public sealed record BootstrapResponse(
    BootstrapUser User,
    IReadOnlyList<string> Permissions);

/// <summary>
/// Сборка ответа <c>/api/bootstrap</c> из cookie-principal.
/// <para>
/// Разбирать access-токен здесь не нужно: аутентификатор кладёт claim'ы <c>permission</c>
/// прямо в principal при входе (см. <see cref="SpaSessionPrincipal"/>).
/// </para>
/// </summary>
public static class BootstrapResponseFactory
{
    /// <summary>Тип claim'а с ключом права.</summary>
    public const string PermissionClaim = "permission";

    public static BootstrapResponse Create(ClaimsPrincipal user)
    {
        var profile = new BootstrapUser(
            Id: user.FindFirst(ApplicationClaimTypes.UserId)?.Value ?? string.Empty,
            UserName: user.FindFirst(ApplicationClaimTypes.UserName)?.Value ?? string.Empty,
            Email: user.FindFirst(ApplicationClaimTypes.Email)?.Value,
            Roles: user.FindAll(ApplicationClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct(StringComparer.Ordinal)
                .ToArray());

        // Identity отдаёт объединение прав ролей и прямых прав — дубли возможны.
        // Сортировка даёт стабильный ответ: меньше лишних диффов на клиенте.
        var permissions = user.FindAll(PermissionClaim)
            .Select(c => c.Value)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new BootstrapResponse(profile, permissions);
    }
}
