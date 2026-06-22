using System.Security.Claims;
using Cheetah.AspNetCore.Blazor.Auth.Tokens;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.AspNetCore.Blazor.Auth;

/// <summary>
/// Server-side AuthenticationStateProvider для Blazor Server. Принципал берётся из
/// аутентифицированного cookie-запроса, поднявшего контур; периодически ревалидируется —
/// если серверная сессия больше не содержит токенов (logout/обновление провалилось),
/// auth-state сбрасывается.
/// </summary>
internal sealed class BffAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IUserTokenStore tokenStore,
    IOptions<BffAuthOptions> options)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => options.Value.RevalidationInterval;

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        var sessionId = authenticationState.User.FindFirst(BffClaimTypes.SessionId)?.Value;
        if (string.IsNullOrEmpty(sessionId))
            return false;

        var tokens = await tokenStore.GetAsync(sessionId, cancellationToken);
        return tokens is not null;
    }
}
