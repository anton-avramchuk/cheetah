using System.Security.Claims;
using Cheetah.Backend.IdentityCore.Domain;
using FluentAssertions;

namespace Cheetah.Backend.IdentityCore.Domain.Tests;

public class IdentityRoleClaimTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateRoleClaim()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var claim = new Claim("Permission", "Users.Create");

        // Act
        var roleClaim = new IdentityRoleClaim(roleId, claim);

        // Assert
        roleClaim.Should().NotBeNull();
        roleClaim.RoleId.Should().Be(roleId);
        roleClaim.ClaimType.Should().Be("Permission");
        roleClaim.ClaimValue.Should().Be("Users.Create");
    }

    [Fact]
    public void Constructor_WithClaimTypeAndValue_ShouldCreateRoleClaim()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var claimType = "Permission";
        var claimValue = "Users.Create";

        // Act
        var roleClaim = new IdentityRoleClaim(roleId, claimType, claimValue);

        // Assert
        roleClaim.Should().NotBeNull();
        roleClaim.RoleId.Should().Be(roleId);
        roleClaim.ClaimType.Should().Be(claimType);
        roleClaim.ClaimValue.Should().Be(claimValue);
    }

    [Fact]
    public void ToClaim_ShouldReturnSystemClaim()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleClaim = new IdentityRoleClaim(roleId, "Permission", "Users.Create");

        // Act
        var claim = roleClaim.ToClaim();

        // Assert
        claim.Should().NotBeNull();
        claim.Type.Should().Be("Permission");
        claim.Value.Should().Be("Users.Create");
    }

    [Fact]
    public void SetClaim_WithValidClaim_ShouldUpdateClaim()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleClaim = new IdentityRoleClaim(roleId, "Permission", "Users.Create");
        var newClaim = new Claim("Permission", "Users.Delete");

        // Act
        roleClaim.SetClaim(newClaim);

        // Assert
        roleClaim.ClaimType.Should().Be("Permission");
        roleClaim.ClaimValue.Should().Be("Users.Delete");
    }

    [Fact]
    public void SetClaim_WithNullClaim_ShouldThrowArgumentNullException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleClaim = new IdentityRoleClaim(roleId, "Permission", "Users.Create");

        // Act
        Action act = () => roleClaim.SetClaim(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}

public class IdentityUserClaimTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUserClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claim = new Claim("Permission", "Reports.View");

        // Act
        var userClaim = new IdentityUserClaim(userId, claim);

        // Assert
        userClaim.Should().NotBeNull();
        userClaim.UserId.Should().Be(userId);
        userClaim.ClaimType.Should().Be("Permission");
        userClaim.ClaimValue.Should().Be("Reports.View");
    }

    [Fact]
    public void Constructor_WithClaimTypeAndValue_ShouldCreateUserClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claimType = "Permission";
        var claimValue = "Reports.View";

        // Act
        var userClaim = new IdentityUserClaim(userId, claimType, claimValue);

        // Assert
        userClaim.Should().NotBeNull();
        userClaim.UserId.Should().Be(userId);
        userClaim.ClaimType.Should().Be(claimType);
        userClaim.ClaimValue.Should().Be(claimValue);
    }

    [Fact]
    public void ToClaim_ShouldReturnSystemClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userClaim = new IdentityUserClaim(userId, "Permission", "Reports.View");

        // Act
        var claim = userClaim.ToClaim();

        // Assert
        claim.Should().NotBeNull();
        claim.Type.Should().Be("Permission");
        claim.Value.Should().Be("Reports.View");
    }

    [Fact]
    public void SetClaim_WithValidClaim_ShouldUpdateClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userClaim = new IdentityUserClaim(userId, "Permission", "Reports.View");
        var newClaim = new Claim("Permission", "Reports.Edit");

        // Act
        userClaim.SetClaim(newClaim);

        // Assert
        userClaim.ClaimType.Should().Be("Permission");
        userClaim.ClaimValue.Should().Be("Reports.Edit");
    }

    [Fact]
    public void SetClaim_WithNullClaim_ShouldThrowArgumentNullException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userClaim = new IdentityUserClaim(userId, "Permission", "Reports.View");

        // Act
        Action act = () => userClaim.SetClaim(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}

public class IdentityUserRoleTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = TestIdentityRole.Create("Admin");

        // Act
        var userRole = new IdentityUserRole<TestIdentityRole>(userId, role);

        // Assert
        userRole.Should().NotBeNull();
        userRole.UserId.Should().Be(userId);
        userRole.RoleId.Should().Be(role.Id);
        userRole.Role.Should().Be(role);
    }

    [Fact]
    public void Constructor_WithNullRole_ShouldThrowArgumentNullException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        Action act = () => new IdentityUserRole<TestIdentityRole>(userId, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetRole_WithValidRole_ShouldUpdateRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role1 = TestIdentityRole.Create("Admin");
        var role2 = TestIdentityRole.Create("User");
        var userRole = new IdentityUserRole<TestIdentityRole>(userId, role1);

        // Act
        userRole.SetRole(role2);

        // Assert
        userRole.RoleId.Should().Be(role2.Id);
        userRole.Role.Should().Be(role2);
    }

    [Fact]
    public void SetRole_WithNullRole_ShouldThrowArgumentNullException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = TestIdentityRole.Create("Admin");
        var userRole = new IdentityUserRole<TestIdentityRole>(userId, role);

        // Act
        Action act = () => userRole.SetRole(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetKeys_ShouldReturnUserIdAndRoleId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = TestIdentityRole.Create("Admin");
        var userRole = new IdentityUserRole<TestIdentityRole>(userId, role);

        // Act
        var keys = userRole.GetKeys();

        // Assert
        keys.Should().HaveCount(2);
        keys[0].Should().Be(userId);
        keys[1].Should().Be(role.Id);
    }
}
