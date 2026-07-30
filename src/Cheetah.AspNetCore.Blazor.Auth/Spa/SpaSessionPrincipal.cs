using System.Security.Claims;
using Cheetah.AspNetCore.Blazor.Auth.Abstractions;
using Cheetah.Core.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Cheetah.AspNetCore.Blazor.Auth.Spa;

/// <summary>
/// Cookie-principal сессии BFF: один и тот же для Blazor-входа (<see cref="BffAuthEndpoints"/>)
/// и для входа SPA (<see cref="SpaAccountEndpoints"/>).
/// <para>
/// Общий он не ради экономии строк: на этом наборе claim'ов держатся сразу
/// <c>GET /api/bootstrap</c>, проверки ролей на группах эндпоинтов и поиск токена в сторе
/// по <see cref="BffClaimTypes.SessionId"/>. Две копии разъезжаются молча, а ломается всё
/// перечисленное разом.
/// </para>
/// </summary>
public static class SpaSessionPrincipal
{
    /// <summary>Собирает principal сессии по профилю пользователя и идентификатору сессии.</summary>
    /// <param name="user">Профиль, полученный от <see cref="Abstractions.IBffAuthenticator"/>.</param>
    /// <param name="sessionId">Ключ, по которому токены лежат в <see cref="Tokens.IUserTokenStore"/>.</param>
    public static ClaimsPrincipal Build(BffUser user, string sessionId)
    {
        var claims = new List<Claim>
        {
            new(ApplicationClaimTypes.UserId, user.Id),
            new(ApplicationClaimTypes.UserName, user.UserName),
            new(BffClaimTypes.SessionId, sessionId),
        };

        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new Claim(ApplicationClaimTypes.Email, user.Email));

        if (user.Roles is not null)
            claims.AddRange(user.Roles.Select(role => new Claim(ApplicationClaimTypes.Role, role)));

        // Права приходят из JWT — без них гейтинг UI сводится к проверке роли.
        if (user.AdditionalClaims is not null)
            claims.AddRange(user.AdditionalClaims);

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ApplicationClaimTypes.UserName,
            ApplicationClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }
}
