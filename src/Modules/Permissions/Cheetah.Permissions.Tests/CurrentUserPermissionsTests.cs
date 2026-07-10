using System.Security.Claims;
using Cheetah.Permissions;
using Microsoft.AspNetCore.Http;
using Shouldly;

using StubFeatureManager = Cheetah.Permissions.Tests.PermissionAuthorizerTests.StubFeatureManager;

namespace Cheetah.Permissions.Tests;

public class CurrentUserPermissionsTests
{
    private static ICurrentUserPermissions Build(
        ClaimsPrincipal? user, Action<PermissionRegistry>? declare = null, Func<string, bool>? featureEnabled = null)
    {
        var accessor = new HttpContextAccessor();
        if (user is not null)
            accessor.HttpContext = new DefaultHttpContext { User = user };

        var registry = new PermissionRegistry();
        declare?.Invoke(registry);
        var authorizer = new ClaimPermissionAuthorizer(registry, new StubFeatureManager(featureEnabled ?? (_ => true)));

        return new CurrentUserPermissions(authorizer, accessor);
    }

    private static ClaimsPrincipal User(params string[] permissions)
    {
        var claims = permissions.Select(p => new Claim(PermissionConstants.PermissionClaimType, p));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Fact]
    public async Task Has_True_When_Claim_Present()
        => (await Build(User("Documents.Sign")).HasAsync("Documents.Sign")).ShouldBeTrue();

    [Fact]
    public async Task Has_False_When_Claim_Missing()
        => (await Build(User("Documents.Read")).HasAsync("Documents.Sign")).ShouldBeFalse();

    [Fact]
    public async Task Has_False_When_No_HttpContext()
        => (await Build(user: null).HasAsync("Documents.Sign")).ShouldBeFalse();

    [Fact]
    public async Task Require_Throws_When_Permission_Missing()
    {
        var sut = Build(User());
        await Should.ThrowAsync<UnauthorizedAccessException>(async () => await sut.RequireAsync("Documents.Sign"));
    }

    [Fact]
    public async Task Require_Does_Not_Throw_When_Permission_Present()
    {
        var sut = Build(User("Documents.Sign"));
        await Should.NotThrowAsync(async () => await sut.RequireAsync("Documents.Sign"));
    }

    /// <summary>Guard внутри хендлера обязан отзывать право так же, как эндпоинт.</summary>
    [Fact]
    public async Task Require_Throws_When_Permissions_Feature_Is_Disabled()
    {
        var sut = Build(
            User("Vacancy.Teams.View"),
            declare: r => r.Add("Vacancy.Teams.View", "", "Vacancy", feature: "Vacancy.Teams"),
            featureEnabled: _ => false);

        await Should.ThrowAsync<UnauthorizedAccessException>(async () => await sut.RequireAsync("Vacancy.Teams.View"));
    }
}
