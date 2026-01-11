using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Events;
using FluentAssertions;

namespace Cheetah.Identity.Domain.Tests;

public class RoleTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateRole()
    {
        // Arrange
        var roleName = "Administrator";
        var description = "Full system access";

        // Act
        var role = Role.Create(roleName, description);

        // Assert
        role.Should().NotBeNull();
        role.Id.Should().NotBe(Guid.Empty);
        role.Name.Should().Be(roleName);
        role.NormalizedName.Should().Be(roleName.ToUpperInvariant());
        role.Description.Should().Be(description);
    }

    [Fact]
    public void Create_WithoutDescription_ShouldCreateRoleWithNullDescription()
    {
        // Arrange
        var roleName = "User";

        // Act
        var role = Role.Create(roleName);

        // Assert
        role.Description.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseRoleCreatedEvent()
    {
        // Arrange
        var roleName = "Admin";

        // Act
        var role = Role.Create(roleName);

        // Assert
        role.DomainEvents.Should().Contain(e => e is RoleCreatedEvent);
        var createdEvent = role.DomainEvents.OfType<RoleCreatedEvent>().First();
        createdEvent.RoleId.Should().Be(role.Id);
        createdEvent.RoleName.Should().Be(roleName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        Action act = () => Role.Create(invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Role name cannot be empty*");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldTrimName()
    {
        // Arrange
        var roleName = "  Admin  ";

        // Act
        var role = Role.Create(roleName);

        // Assert
        role.Name.Should().Be("Admin");
    }

    [Fact]
    public void AddPermission_WithValidPermission_ShouldAddPermission()
    {
        // Arrange
        var role = Role.Create("Admin");
        var permission = "Users.Create";

        // Act
        role.AddPermission(permission);

        // Assert
        role.Claims.Should().HaveCount(1);
        role.HasPermission(permission).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddPermission_WithEmptyPermission_ShouldThrowArgumentException(string? invalidPermission)
    {
        // Arrange
        var role = Role.Create("Admin");

        // Act
        Action act = () => role.AddPermission(invalidPermission!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Permission cannot be empty*");
    }

    [Fact]
    public void AddPermission_WithDuplicatePermission_ShouldNotAddAgain()
    {
        // Arrange
        var role = Role.Create("Admin");
        var permission = "Users.Create";
        role.AddPermission(permission);

        // Act
        role.AddPermission(permission);

        // Assert
        role.Claims.Should().HaveCount(1);
    }

    [Fact]
    public void AddPermission_ShouldRaiseRoleClaimAddedEvent()
    {
        // Arrange
        var role = Role.Create("Admin");
        var permission = "Users.Create";
        role.ClearDomainEvents();

        // Act
        role.AddPermission(permission);

        // Assert
        role.DomainEvents.Should().Contain(e => e is RoleClaimAddedEvent);
        var claimAddedEvent = role.DomainEvents.OfType<RoleClaimAddedEvent>().First();
        claimAddedEvent.RoleId.Should().Be(role.Id);
        claimAddedEvent.ClaimType.Should().Be("Permission");
        claimAddedEvent.ClaimValue.Should().Be(permission);
    }

    [Fact]
    public void RemovePermission_WithExistingPermission_ShouldRemovePermission()
    {
        // Arrange
        var role = Role.Create("Admin");
        var permission = "Users.Create";
        role.AddPermission(permission);

        // Act
        role.RemovePermission(permission);

        // Assert
        role.HasPermission(permission).Should().BeFalse();
        role.Claims.Should().BeEmpty();
    }

    [Fact]
    public void RemovePermission_WithNonExistingPermission_ShouldNotThrow()
    {
        // Arrange
        var role = Role.Create("Admin");

        // Act
        Action act = () => role.RemovePermission("NonExisting.Permission");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GetPermissions_ShouldReturnOnlyPermissionClaims()
    {
        // Arrange
        var role = Role.Create("Admin");
        role.AddPermission("Users.Create");
        role.AddPermission("Users.Delete");
        role.AddClaim("CustomClaim", "CustomValue");

        // Act
        var permissions = role.GetPermissions().ToList();

        // Assert
        permissions.Should().HaveCount(2);
        permissions.Should().Contain("Users.Create");
        permissions.Should().Contain("Users.Delete");
        permissions.Should().NotContain("CustomValue");
    }

    [Fact]
    public void HasPermission_WithExistingPermission_ShouldReturnTrue()
    {
        // Arrange
        var role = Role.Create("Admin");
        var permission = "Users.Create";
        role.AddPermission(permission);

        // Act
        var result = role.HasPermission(permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPermission_WithNonExistingPermission_ShouldReturnFalse()
    {
        // Arrange
        var role = Role.Create("Admin");

        // Act
        var result = role.HasPermission("NonExisting.Permission");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AddClaim_WithValidClaim_ShouldAddClaim()
    {
        // Arrange
        var role = Role.Create("Admin");
        var claimType = "Department";
        var claimValue = "IT";

        // Act
        role.AddClaim(claimType, claimValue);

        // Assert
        role.Claims.Should().HaveCount(1);
        var claim = role.Claims.First();
        claim.ClaimType.Should().Be(claimType);
        claim.ClaimValue.Should().Be(claimValue);
    }

    [Theory]
    [InlineData("", "value")]
    [InlineData(" ", "value")]
    [InlineData(null, "value")]
    public void AddClaim_WithEmptyClaimType_ShouldThrowArgumentException(string? invalidType, string value)
    {
        // Arrange
        var role = Role.Create("Admin");

        // Act
        Action act = () => role.AddClaim(invalidType!, value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Claim type cannot be empty*");
    }

    [Theory]
    [InlineData("type", "")]
    [InlineData("type", " ")]
    [InlineData("type", null)]
    public void AddClaim_WithEmptyClaimValue_ShouldThrowArgumentException(string type, string? invalidValue)
    {
        // Arrange
        var role = Role.Create("Admin");

        // Act
        Action act = () => role.AddClaim(type, invalidValue!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Claim value cannot be empty*");
    }

    [Fact]
    public void AddClaim_WithDuplicateClaim_ShouldNotAddAgain()
    {
        // Arrange
        var role = Role.Create("Admin");
        var claimType = "Department";
        var claimValue = "IT";
        role.AddClaim(claimType, claimValue);

        // Act
        role.AddClaim(claimType, claimValue);

        // Assert
        role.Claims.Should().HaveCount(1);
    }

    [Fact]
    public void AddClaim_ShouldRaiseRoleClaimAddedEvent()
    {
        // Arrange
        var role = Role.Create("Admin");
        var claimType = "Department";
        var claimValue = "IT";
        role.ClearDomainEvents();

        // Act
        role.AddClaim(claimType, claimValue);

        // Assert
        role.DomainEvents.Should().Contain(e => e is RoleClaimAddedEvent);
        var claimAddedEvent = role.DomainEvents.OfType<RoleClaimAddedEvent>().First();
        claimAddedEvent.RoleId.Should().Be(role.Id);
        claimAddedEvent.ClaimType.Should().Be(claimType);
        claimAddedEvent.ClaimValue.Should().Be(claimValue);
    }

    [Fact]
    public void RemoveClaim_WithExistingClaim_ShouldRemoveClaim()
    {
        // Arrange
        var role = Role.Create("Admin");
        var claimType = "Department";
        var claimValue = "IT";
        role.AddClaim(claimType, claimValue);

        // Act
        role.RemoveClaim(claimType, claimValue);

        // Assert
        role.Claims.Should().BeEmpty();
    }

    [Fact]
    public void RemoveClaim_WithNonExistingClaim_ShouldNotThrow()
    {
        // Arrange
        var role = Role.Create("Admin");

        // Act
        Action act = () => role.RemoveClaim("NonExisting", "Value");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateRoleInformation()
    {
        // Arrange
        var role = Role.Create("Admin", "Old description");
        var newName = "SuperAdmin";
        var newDescription = "New description";

        // Act
        role.Update(newName, newDescription);

        // Assert
        role.Name.Should().Be(newName);
        role.NormalizedName.Should().Be(newName.ToUpperInvariant());
        role.Description.Should().Be(newDescription);
    }

    [Fact]
    public void Update_WithWhitespace_ShouldTrimValues()
    {
        // Arrange
        var role = Role.Create("Admin");

        // Act
        role.Update("  SuperAdmin  ", "  New description  ");

        // Assert
        role.Name.Should().Be("SuperAdmin");
        role.Description.Should().Be("New description");
    }

    [Fact]
    public void Update_WithoutDescription_ShouldSetDescriptionToNull()
    {
        // Arrange
        var role = Role.Create("Admin", "Old description");

        // Act
        role.Update("Admin");

        // Assert
        role.Description.Should().BeNull();
    }
}
