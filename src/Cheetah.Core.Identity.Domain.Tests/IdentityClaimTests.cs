namespace Cheetah.Core.Identity.Domain.Tests;

/// <summary>
/// Tests for IdentityClaim behaviour accessed through aggregate roots.
/// Claims are internal entities mutated only by their aggregates;
/// SetClaim() is internal and exercised via user.ReplaceClaim() / role tests.
/// </summary>
public class IdentityClaimTests
{
    // --- IdentityUserClaim (via IdentityUser) ---

    [Fact]
    public void UserClaim_HasCorrectTypeAndValue()
    {
        var user = IdentityUser<IdentityRole>.Create("john", "john@example.com");
        user.AddClaim(new Claim("role", "admin"));

        var claim = user.Claims.First();

        claim.ClaimType.ShouldBe("role");
        claim.ClaimValue.ShouldBe("admin");
    }

    [Fact]
    public void UserClaim_HasCorrectUserId()
    {
        var user = IdentityUser<IdentityRole>.Create("john", "john@example.com");
        user.AddClaim(new Claim("role", "admin"));

        user.Claims.First().UserId.ShouldBe(user.Id);
    }

    [Fact]
    public void UserClaim_GeneratesNewId_ForEachClaim()
    {
        var user = IdentityUser<IdentityRole>.Create("john", "john@example.com");
        user.AddClaim(new Claim("role", "admin"));
        user.AddClaim(new Claim("role", "user"));

        user.Claims.Select(c => c.Id).Distinct().Count().ShouldBe(2);
    }

    [Fact]
    public void UserClaim_ToClaim_ReturnsClaim_WithCorrectTypeAndValue()
    {
        var user = IdentityUser<IdentityRole>.Create("john", "john@example.com");
        user.AddClaim(new Claim("role", "admin"));

        var systemClaim = user.Claims.First().ToClaim();

        systemClaim.Type.ShouldBe("role");
        systemClaim.Value.ShouldBe("admin");
    }

    // SetClaim() is internal — mutation is tested via user.ReplaceClaim() in IdentityUserTests.

    // --- IdentityRoleClaim (via IdentityRole) ---

    [Fact]
    public void RoleClaim_HasCorrectTypeAndValue()
    {
        var role = IdentityRole.Create("admin");
        role.AddClaim(new Claim("permission", "read"));

        var claim = role.Claims.First();

        claim.ClaimType.ShouldBe("permission");
        claim.ClaimValue.ShouldBe("read");
    }

    [Fact]
    public void RoleClaim_HasCorrectRoleId()
    {
        var role = IdentityRole.Create("admin");
        role.AddClaim(new Claim("permission", "read"));

        role.Claims.First().RoleId.ShouldBe(role.Id);
    }

    [Fact]
    public void RoleClaim_ToClaim_ReturnsClaim_WithCorrectTypeAndValue()
    {
        var role = IdentityRole.Create("admin");
        role.AddClaim(new Claim("permission", "read"));

        var systemClaim = role.Claims.First().ToClaim();

        systemClaim.Type.ShouldBe("permission");
        systemClaim.Value.ShouldBe("read");
    }
}
