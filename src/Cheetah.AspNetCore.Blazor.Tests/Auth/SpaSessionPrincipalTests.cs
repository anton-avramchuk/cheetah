using System.Security.Claims;
using Cheetah.AspNetCore.Blazor.Auth;
using Cheetah.AspNetCore.Blazor.Auth.Abstractions;
using Cheetah.AspNetCore.Blazor.Auth.Spa;
using Cheetah.Core.Security.Claims;

namespace Cheetah.AspNetCore.Blazor.Tests.Auth;

/// <summary>
/// Cookie-principal сессии SPA.
/// <para>
/// Тот же principal строит и Blazor-вход <see cref="BffAuthEndpoints"/> — теперь буквально
/// этим же кодом. На нём держатся <c>GET /api/bootstrap</c>, проверки ролей на группе
/// <c>/api</c> и поиск токена в сторе по <see cref="BffClaimTypes.SessionId"/>, поэтому
/// набор claim'ов закреплён тестами.
/// </para>
/// </summary>
public sealed class SpaSessionPrincipalTests
{
    private const string PermissionClaim = "permission";

    private static BffUser User(
        IReadOnlyList<string>? roles = null,
        IReadOnlyList<Claim>? additional = null,
        string? email = "admin@admin.com")
        => new(
            Id: "11111111-1111-1111-1111-111111111111",
            UserName: "admin@admin.com",
            Email: email,
            Roles: roles,
            AdditionalClaims: additional);

    [Fact]
    public void Principal_is_authenticated_with_cookie_scheme()
    {
        var principal = SpaSessionPrincipal.Build(User(), "session-1");

        principal.Identity!.IsAuthenticated.ShouldBeTrue();
    }

    [Fact]
    public void Carries_identity_of_the_user()
    {
        var principal = SpaSessionPrincipal.Build(User(), "session-1");

        principal.FindFirst(ApplicationClaimTypes.UserId)!.Value.ShouldBe("11111111-1111-1111-1111-111111111111");
        principal.FindFirst(ApplicationClaimTypes.UserName)!.Value.ShouldBe("admin@admin.com");
        principal.FindFirst(ApplicationClaimTypes.Email)!.Value.ShouldBe("admin@admin.com");
    }

    [Fact]
    public void Session_id_claim_links_principal_to_the_token_store()
    {
        // Без него токен сессии не найти и все /api/* ответят 401.
        var principal = SpaSessionPrincipal.Build(User(), "session-1");

        principal.FindFirst(BffClaimTypes.SessionId)!.Value.ShouldBe("session-1");
    }

    [Fact]
    public void Roles_are_recognized_by_IsInRole()
    {
        // Группы эндпоинтов висят на RequireRole — роль должна читаться штатным способом.
        var principal = SpaSessionPrincipal.Build(User(roles: ["admin", "owner"]), "session-1");

        principal.IsInRole("admin").ShouldBeTrue();
        principal.IsInRole("employer").ShouldBeFalse();
    }

    [Fact]
    public void Permissions_from_token_land_in_principal()
    {
        // Гранулярный гейтинг SPA строится на этих claim'ах через /api/bootstrap.
        var permissions = new Claim[]
        {
            new(PermissionClaim, "vacancies.view"),
            new(PermissionClaim, "candidates.view"),
        };

        var principal = SpaSessionPrincipal.Build(User(additional: permissions), "session-1");

        principal.FindAll(PermissionClaim).Select(c => c.Value)
            .ShouldBe(["vacancies.view", "candidates.view"], ignoreOrder: true);
    }

    [Fact]
    public void Email_is_omitted_when_user_has_none()
    {
        var principal = SpaSessionPrincipal.Build(User(email: null), "session-1");

        principal.FindFirst(ApplicationClaimTypes.Email).ShouldBeNull();
    }

    [Fact]
    public void Bootstrap_reads_the_same_principal_back()
    {
        // Страховка от рассинхрона: вход SPA и ответ /api/bootstrap используют одни типы claim'ов.
        var principal = SpaSessionPrincipal.Build(
            User(roles: ["admin"], additional: [new Claim(PermissionClaim, "vacancies.view")]),
            "session-1");

        var response = BootstrapResponseFactory.Create(principal);

        response.User.UserName.ShouldBe("admin@admin.com");
        response.User.Roles.ShouldBe(["admin"]);
        response.Permissions.ShouldBe(["vacancies.view"]);
    }
}
