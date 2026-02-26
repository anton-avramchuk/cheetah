using System.Security.Claims;
using System.Text.Json;
using Cheetah.Core.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;

namespace Cheetah.Frontend.Auth;

[Export(LifetimeType.Scoped, typeof(AuthenticationStateProvider), typeof(JwtAuthenticationStateProvider), typeof(IAuthStateNotifier))]
public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider, IAuthStateNotifier
{
    private readonly ITokenStorage _tokenStorage;

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthenticationStateProvider(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        if (IsTokenExpired(token))
        {
            await _tokenStorage.RemoveTokenAsync();
            return Anonymous;
        }

        return new AuthenticationState(new ClaimsPrincipal(BuildIdentity(token)));
    }

    public async Task NotifyLoginAsync(string token)
    {
        await _tokenStorage.SetTokenAsync(token);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(BuildIdentity(token)))));
    }

    public async Task NotifyLogoutAsync()
    {
        await _tokenStorage.RemoveTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private static bool IsTokenExpired(string token)
    {
        var exp = ParseClaimsFromJwt(token)
            .FirstOrDefault(c => c.Type == "exp")?.Value;

        // Токен без exp или с невалидным exp считается просроченным — fail-closed.
        if (exp is null || !long.TryParse(exp, out var expSeconds))
            return true;

        return DateTimeOffset.FromUnixTimeSeconds(expSeconds) < DateTimeOffset.UtcNow;
    }

    private static ClaimsIdentity BuildIdentity(string token) =>
        new(ParseClaimsFromJwt(token), "jwt", nameType: "sub", roleType: "role");

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3)
            return [];

        var payload = parts[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var base64 = padded.Replace('-', '+').Replace('_', '/');

        try
        {
            var jsonBytes = Convert.FromBase64String(base64);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
            if (dict is null) return [];

            return dict.SelectMany(kvp => kvp.Value.ValueKind == JsonValueKind.Array
                ? kvp.Value.EnumerateArray().Select(el => new Claim(kvp.Key, el.GetString() ?? ""))
                : [new Claim(kvp.Key, kvp.Value.ToString())]);
        }
        catch
        {
            // Намеренно молчим: невалидный JWT → пустые claims → пользователь Anonymous.
            // Это безопасное поведение, хотя затрудняет отладку.
            return [];
        }
    }
}
