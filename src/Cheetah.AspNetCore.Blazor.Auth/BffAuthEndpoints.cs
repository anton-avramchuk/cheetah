using System.Security.Claims;
using Cheetah.AspNetCore.Blazor.Auth.Abstractions;
using Cheetah.AspNetCore.Blazor.Auth.Tokens;
using Cheetah.Core.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Cheetah.AspNetCore.Blazor.Auth;

/// <summary>
/// HTTP-эндпоинты входа/выхода. В Blazor Server вход нельзя выполнить в интерактивном контуре
/// (cookie пишется только в HttpContext), поэтому форма логина делает нативный POST сюда.
/// </summary>
public static class BffAuthEndpoints
{
    public static IEndpointRouteBuilder MapBffAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/account");

        group.MapPost("/login", async (
            [FromForm] string userName,
            [FromForm] string password,
            [FromForm] string? returnUrl,
            [FromServices] IBffAuthenticator authenticator,
            [FromServices] IUserTokenStore tokenStore,
            [FromServices] IOptions<BffAuthOptions> options,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var safeReturn = !string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith('/') ? returnUrl : "/";
            var loginPath = options.Value.LoginPath;

            var result = await authenticator.AuthenticateAsync(new BffCredentials(userName, password), ct);
            if (!result.Succeeded || result.User is null || result.Tokens is null)
                return Results.LocalRedirect($"{loginPath}?error=1&returnUrl={Uri.EscapeDataString(safeReturn)}");

            var sessionId = Guid.NewGuid().ToString("N");
            await tokenStore.StoreAsync(sessionId, result.Tokens, ct);

            var principal = BuildPrincipal(result.User, sessionId);
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });

            return Results.LocalRedirect(safeReturn);
        }).DisableAntiforgery().AllowAnonymous();

        group.MapPost("/logout", async (
            [FromServices] IUserTokenStore tokenStore,
            [FromServices] IOptions<BffAuthOptions> options,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var sessionId = httpContext.User.FindFirst(BffClaimTypes.SessionId)?.Value;
            if (!string.IsNullOrEmpty(sessionId))
                await tokenStore.RemoveAsync(sessionId, ct);

            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.LocalRedirect(options.Value.LoginPath);
        }).DisableAntiforgery();

        return endpoints;
    }

    private static ClaimsPrincipal BuildPrincipal(BffUser user, string sessionId)
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
            claims.AddRange(user.Roles.Select(r => new Claim(ApplicationClaimTypes.Role, r)));

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
