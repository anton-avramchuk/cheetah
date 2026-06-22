using Cheetah.AspNetCore.Blazor.Auth.Abstractions;
using Cheetah.Core.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace Cheetah.AspNetCore.Blazor.Auth.Tokens;

[Export(LifetimeType.Scoped, typeof(IAccessTokenProvider))]
public sealed class AccessTokenProvider(
    AuthenticationStateProvider authenticationStateProvider,
    IUserTokenStore tokenStore,
    IBffAuthenticator authenticator,
    IOptions<BffAuthOptions> options,
    TimeProvider timeProvider) : IAccessTokenProvider
{
    public async Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var sessionId = await GetSessionIdAsync();
        if (sessionId is null)
            return null;

        var tokens = await tokenStore.GetAsync(sessionId, ct);
        if (tokens is null)
            return null;

        var skew = TimeSpan.FromSeconds(options.Value.RefreshSkewSeconds);
        if (tokens.ExpiresAt - skew > timeProvider.GetUtcNow())
            return tokens.AccessToken;

        // Токен протух (с учётом запаса) — пробуем обновить.
        var refreshed = await RefreshAsync(sessionId, tokens, ct);
        return refreshed?.AccessToken;
    }

    public async Task<string?> RefreshAccessTokenAsync(CancellationToken ct = default)
    {
        var sessionId = await GetSessionIdAsync();
        if (sessionId is null)
            return null;

        var tokens = await tokenStore.GetAsync(sessionId, ct);
        if (tokens is null)
            return null;

        var refreshed = await RefreshAsync(sessionId, tokens, ct);
        return refreshed?.AccessToken;
    }

    private async Task<BffTokenSet?> RefreshAsync(string sessionId, BffTokenSet current, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(current.RefreshToken))
        {
            await tokenStore.RemoveAsync(sessionId, ct);
            return null;
        }

        var refreshed = await authenticator.RefreshAsync(current.RefreshToken, ct);
        if (refreshed is null)
        {
            await tokenStore.RemoveAsync(sessionId, ct);
            return null;
        }

        await tokenStore.StoreAsync(sessionId, refreshed, ct);
        return refreshed;
    }

    private async Task<string?> GetSessionIdAsync()
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        return state.User.FindFirst(BffClaimTypes.SessionId)?.Value;
    }
}
