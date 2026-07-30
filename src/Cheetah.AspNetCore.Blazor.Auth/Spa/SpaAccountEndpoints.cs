using System.Security.Claims;
using Cheetah.AspNetCore.Blazor.Auth.Abstractions;
using Cheetah.AspNetCore.Blazor.Auth.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.AspNetCore.Blazor.Auth.Spa;

/// <summary>Учётные данные входа из SPA.</summary>
public sealed record SpaLoginRequest(string UserName, string Password);

/// <summary>
/// Вход, выход и bootstrap для SPA поверх той же модели cookie-BFF, что и Blazor-вход.
/// <para>
/// Штатные <c>/account/login|logout</c> из <see cref="BffAuthEndpoints"/> для SPA не годятся:
/// они принимают HTML-форму и отвечают редиректом, а редирект для <c>fetch</c> неотличим
/// от успеха — клиент не поймёт, что вход не удался. Здесь тот же сценарий, но JSON
/// и честные коды состояния. Токен по-прежнему живёт в сторе на сервере, наружу уходит
/// только сессионный cookie.
/// </para>
/// <para>
/// CSRF: тело читается только как JSON, поэтому кросс-сайтовой HTML-формой (единственный
/// запрос, который браузер отправит без preflight) вход не подделать — форма получит 415.
/// CORS включать не нужно: SPA раздаётся тем же хостом, а в разработке ходит через proxy
/// dev-сервера, то есть запросы всегда с того же origin.
/// </para>
/// </summary>
public static class SpaAccountEndpoints
{
    /// <summary>
    /// Мапит <c>POST /api/account/login</c> и <c>POST /api/account/logout</c>.
    /// </summary>
    /// <param name="endpoints">Куда добавлять маршруты.</param>
    /// <param name="tag">Тег OpenAPI — по нему генератор клиента раскладывает методы по сервисам.</param>
    public static IEndpointRouteBuilder MapSpaAccountApi(
        this IEndpointRouteBuilder endpoints,
        string tag = "BffAccount")
    {
        var account = endpoints.MapGroup("/api/account")
            .WithTags(tag)
            .DisableAntiforgery();

        account.MapPost("/login", async (
            [FromBody] SpaLoginRequest request,
            [FromServices] IBffAuthenticator authenticator,
            [FromServices] IUserTokenStore tokenStore,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await authenticator.AuthenticateAsync(
                new BffCredentials(request.UserName, request.Password), ct);

            if (!result.Succeeded || result.User is null || result.Tokens is null)
                return Results.Problem(
                    title: "Неверный логин или пароль",
                    statusCode: StatusCodes.Status401Unauthorized);

            var sessionId = Guid.NewGuid().ToString("N");
            await tokenStore.StoreAsync(sessionId, result.Tokens, ct);

            var principal = SpaSessionPrincipal.Build(result.User, sessionId);
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });

            // Профиль тем же ответом: SPA не нужен второй запрос к /api/bootstrap сразу после входа.
            return Results.Ok(BootstrapResponseFactory.Create(principal));
        })
            .AllowAnonymous()
            .Produces<BootstrapResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName("Login")
            .WithSummary("Вход: заводит cookie-сессию и возвращает профиль");

        account.MapPost("/logout", async (
            [FromServices] IUserTokenStore tokenStore,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            // Токен из стора удаляем сами: cookie погаснет, а запись переживёт выход до конца TTL.
            var sessionId = httpContext.User.FindFirst(BffClaimTypes.SessionId)?.Value;
            if (!string.IsNullOrEmpty(sessionId))
                await tokenStore.RemoveAsync(sessionId, ct);

            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        })
            .AllowAnonymous() // выход из уже протухшей сессии не должен отвечать 401
            .WithName("Logout")
            .WithSummary("Выход: гасит cookie-сессию и удаляет токен из стора");

        return endpoints;
    }

    /// <summary>
    /// Мапит <c>GET /bootstrap</c> в переданную группу — единственный вызов при инициализации SPA.
    /// <para>
    /// Именно в группу, а не в корень: ответ описывает текущую сессию, поэтому маршрут должен
    /// попасть под ту же авторизацию, что и остальной API приложения.
    /// </para>
    /// </summary>
    public static IEndpointRouteBuilder MapSpaBootstrap(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/bootstrap", (ClaimsPrincipal user) => Results.Ok(BootstrapResponseFactory.Create(user)))
            .Produces<BootstrapResponse>()
            .WithName("GetBootstrap")
            .WithSummary("Профиль текущей сессии: пользователь, роли, права");

        return endpoints;
    }
}
