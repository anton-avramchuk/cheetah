using System.Security.Claims;
using Cheetah.Permissions;
using Shouldly;

namespace Cheetah.Permissions.Tests;

public class PermissionAuthorizerTests
{
    private readonly IPermissionAuthorizer _sut = new ClaimPermissionAuthorizer();

    private static ClaimsPrincipal User(params string[] permissions)
    {
        var claims = permissions.Select(p => new Claim(PermissionConstants.PermissionClaimType, p));
        var identity = new ClaimsIdentity(claims, authenticationType: "test");
        return new ClaimsPrincipal(identity);
    }

    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    [Fact]
    public void Has_Returns_True_When_Claim_Present()
    {
        _sut.Has(User("Documents.Sign"), "Documents.Sign").ShouldBeTrue();
    }

    [Fact]
    public void Has_Returns_False_When_Claim_Absent()
    {
        _sut.Has(User("Documents.Read"), "Documents.Sign").ShouldBeFalse();
    }

    [Fact]
    public void Has_Returns_False_For_Anonymous_User()
    {
        _sut.Has(Anonymous, "Documents.Sign").ShouldBeFalse();
    }

    [Fact]
    public void HasAll_True_Only_When_All_Present()
    {
        var u = User("A", "B", "C");
        _sut.HasAll(u, new[] { "A", "B" }).ShouldBeTrue();
        _sut.HasAll(u, new[] { "A", "B", "D" }).ShouldBeFalse();
    }

    [Fact]
    public void HasAny_True_When_At_Least_One_Present()
    {
        var u = User("A");
        _sut.HasAny(u, new[] { "A", "B" }).ShouldBeTrue();
        _sut.HasAny(u, new[] { "B", "C" }).ShouldBeFalse();
    }
}
