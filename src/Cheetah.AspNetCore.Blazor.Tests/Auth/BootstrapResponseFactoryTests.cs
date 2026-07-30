using System.Security.Claims;
using Cheetah.AspNetCore.Blazor.Auth.Spa;
using Cheetah.Core.Security.Claims;

namespace Cheetah.AspNetCore.Blazor.Tests.Auth;

/// <summary>
/// <c>GET /api/bootstrap</c> — единственный вызов, которым SPA получает всё для гейтинга UI.
/// Источник — cookie-principal: в нём есть и роли, и права, поэтому разбирать access-токен
/// здесь не нужно.
/// </summary>
public sealed class BootstrapResponseFactoryTests
{
    private const string PermissionClaim = "permission";

    private static ClaimsPrincipal User(params Claim[] claims)
        => new(new ClaimsIdentity(
            claims,
            authenticationType: "Cookies",
            nameType: ApplicationClaimTypes.UserName,
            roleType: ApplicationClaimTypes.Role));

    [Fact]
    public void Maps_identity_fields_from_principal()
    {
        var user = User(
            new Claim(ApplicationClaimTypes.UserId, "11111111-1111-1111-1111-111111111111"),
            new Claim(ApplicationClaimTypes.UserName, "admin@admin.com"),
            new Claim(ApplicationClaimTypes.Email, "admin@admin.com"));

        var response = BootstrapResponseFactory.Create(user);

        response.User.Id.ShouldBe("11111111-1111-1111-1111-111111111111");
        response.User.UserName.ShouldBe("admin@admin.com");
        response.User.Email.ShouldBe("admin@admin.com");
    }

    [Fact]
    public void Collects_roles_and_permissions()
    {
        var user = User(
            new Claim(ApplicationClaimTypes.UserName, "u"),
            new Claim(ApplicationClaimTypes.Role, "admin"),
            new Claim(ApplicationClaimTypes.Role, "owner"),
            new Claim(PermissionClaim, "vacancies.view"),
            new Claim(PermissionClaim, "candidates.view"));

        var response = BootstrapResponseFactory.Create(user);

        response.User.Roles.ShouldBe(["admin", "owner"], ignoreOrder: true);
        response.Permissions.ShouldBe(["vacancies.view", "candidates.view"], ignoreOrder: true);
    }

    [Fact]
    public void Deduplicates_permissions()
    {
        // Identity отдаёт объединение прав ролей и прямых прав — пересечение возможно.
        var user = User(
            new Claim(ApplicationClaimTypes.UserName, "u"),
            new Claim(PermissionClaim, "vacancies.view"),
            new Claim(PermissionClaim, "vacancies.view"));

        BootstrapResponseFactory.Create(user).Permissions.ShouldBe(["vacancies.view"]);
    }

    [Fact]
    public void Missing_optional_claims_do_not_throw()
    {
        var user = User(new Claim(ApplicationClaimTypes.UserName, "u"));

        var response = BootstrapResponseFactory.Create(user);

        response.User.Id.ShouldBe(string.Empty);
        response.User.Email.ShouldBeNull();
        response.User.Roles.ShouldBeEmpty();
        response.Permissions.ShouldBeEmpty();
    }

    [Fact]
    public void Permissions_are_sorted_for_stable_payload()
    {
        // Стабильный порядок = стабильный дифф на клиенте, меньше лишних ре-рендеров.
        var user = User(
            new Claim(ApplicationClaimTypes.UserName, "u"),
            new Claim(PermissionClaim, "b.view"),
            new Claim(PermissionClaim, "a.view"));

        BootstrapResponseFactory.Create(user).Permissions.ShouldBe(["a.view", "b.view"]);
    }
}
