using System.Security.Claims;
using Cheetah.Permissions;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Cheetah.Permissions.Tests;

public class CurrentUserPermissionsTests
{
    private static ICurrentUserPermissions Build(ClaimsPrincipal? user)
    {
        var accessor = new HttpContextAccessor();
        if (user is not null)
            accessor.HttpContext = new DefaultHttpContext { User = user };

        return new CurrentUserPermissions(new ClaimPermissionAuthorizer(), accessor);
    }

    private static ClaimsPrincipal User(params string[] permissions)
    {
        var claims = permissions.Select(p => new Claim(PermissionConstants.PermissionClaimType, p));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Fact]
    public void Has_True_When_Claim_Present()
    {
        var sut = Build(User("Documents.Sign"));
        sut.Has("Documents.Sign").ShouldBeTrue();
    }

    [Fact]
    public void Has_False_When_Claim_Missing()
    {
        var sut = Build(User("Documents.Read"));
        sut.Has("Documents.Sign").ShouldBeFalse();
    }

    [Fact]
    public void Has_False_When_No_HttpContext()
    {
        var sut = Build(user: null);
        sut.Has("Documents.Sign").ShouldBeFalse();
    }

    [Fact]
    public void Require_Throws_When_Permission_Missing()
    {
        var sut = Build(User());
        Should.Throw<UnauthorizedAccessException>(() => sut.Require("Documents.Sign"));
    }

    [Fact]
    public void Require_Does_Not_Throw_When_Permission_Present()
    {
        var sut = Build(User("Documents.Sign"));
        Should.NotThrow(() => sut.Require("Documents.Sign"));
    }
}
