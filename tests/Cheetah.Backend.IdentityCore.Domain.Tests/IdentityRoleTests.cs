using System.Security.Claims;
using Cheetah.Backend.IdentityCore.Domain;
using FluentAssertions;

namespace Cheetah.Backend.IdentityCore.Domain.Tests;

// Test implementation of IdentityRole for testing purposes
public class TestIdentityRole : IdentityRole
{
    private TestIdentityRole() { }

    private TestIdentityRole(string name) : base(name) { }

    private TestIdentityRole(Guid id, string name) : base(id, name) { }

    public static TestIdentityRole Create(string name)
    {
        return new TestIdentityRole(name);
    }

    public static TestIdentityRole Create(Guid id, string name)
    {
        return new TestIdentityRole(id, name);
    }
}

public class IdentityRoleTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateRole()
    {
        // Arrange
        var roleName = "Admin";

        // Act
        var role = TestIdentityRole.Create(roleName);

        // Assert
        role.Should().NotBeNull();
        role.Id.Should().NotBe(Guid.Empty);
        role.Name.Should().Be(roleName);
        role.NormalizedName.Should().Be(roleName.ToUpperInvariant());
    }

    [Fact]
    public void Create_WithSpecificId_ShouldUseProvidedId()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleName = "Admin";

        // Act
        var role = TestIdentityRole.Create(roleId, roleName);

        // Assert
        role.Id.Should().Be(roleId);
        role.Name.Should().Be(roleName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        Action act = () => TestIdentityRole.Create(invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Role name cannot be empty*");
    }

    [Fact]
    public void ChangeName_WithValidName_ShouldUpdateNameAndNormalizedName()
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");
        var newName = "SuperAdmin";

        // Act
        role.ChangeName(newName);

        // Assert
        role.Name.Should().Be(newName);
        role.NormalizedName.Should().Be(newName.ToUpperInvariant());
    }

    [Fact]
    public void ChangeName_ShouldRaiseDomainEvent()
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");
        var newName = "SuperAdmin";
        role.ClearDomainEvents();

        // Act
        role.ChangeName(newName);

        // Assert
        role.DomainEvents.Should().HaveCount(1);
        var domainEvent = role.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.RoleNameChangedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ChangeName_WithEmptyName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");

        // Act
        Action act = () => role.ChangeName(invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Role name cannot be empty*");
    }

    [Fact]
    public void AddClaim_WithValidClaim_ShouldAddClaimToRole()
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");
        var claim = new Claim("Permission", "Users.Create");

        // Act
        role.AddClaim(claim);

        // Assert
        role.Claims.Should().HaveCount(1);
        var roleClaim = role.Claims.First();
        roleClaim.ClaimType.Should().Be("Permission");
        roleClaim.ClaimValue.Should().Be("Users.Create");
        roleClaim.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void AddClaim_ShouldRaiseDomainEvent()
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");
        var claim = new Claim("Permission", "Users.Create");
        role.ClearDomainEvents();

        // Act
        role.AddClaim(claim);

        // Assert
        role.DomainEvents.Should().HaveCount(1);
        var domainEvent = role.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.RoleClaimAddedEvent>();
    }

    [Fact]
    public void AddClaim_WithNullClaim_ShouldThrowArgumentNullException()
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");

        // Act
        Action act = () => role.AddClaim(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveClaim_WithExistingClaim_ShouldRemoveClaimFromRole()
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");
        var claim = new Claim("Permission", "Users.Create");
        role.AddClaim(claim);

        // Act
        role.RemoveClaim(claim);

        // Assert
        role.Claims.Should().BeEmpty();
    }

    [Fact]
    public void RemoveClaim_ShouldRaiseDomainEvent()
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");
        var claim = new Claim("Permission", "Users.Create");
        role.AddClaim(claim);
        role.ClearDomainEvents();

        // Act
        role.RemoveClaim(claim);

        // Assert
        role.DomainEvents.Should().HaveCount(1);
        var domainEvent = role.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.RoleClaimRemovedEvent>();
    }

    [Fact]
    public void RemoveClaim_WithNonExistingClaim_ShouldNotThrow()
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");
        var claim = new Claim("Permission", "Users.Create");

        // Act
        Action act = () => role.RemoveClaim(claim);

        // Assert
        act.Should().NotThrow();
        role.Claims.Should().BeEmpty();
    }

    [Fact]
    public void RemoveClaim_WithNullClaim_ShouldThrowArgumentNullException()
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");

        // Act
        Action act = () => role.RemoveClaim(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var role = TestIdentityRole.Create("Admin");

        // Act
        var result = role.ToString();

        // Assert
        result.Should().Contain("Admin");
    }

    [Fact]
    public void CreatedAt_ShouldBeSet()
    {
        // Arrange & Act
        var role = TestIdentityRole.Create("Admin");
        role.CreatedAt = DateTimeOffset.UtcNow;

        // Assert
        role.CreatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatedAt_ShouldBeSet()
    {
        // Arrange & Act
        var role = TestIdentityRole.Create("Admin");
        role.UpdatedAt = DateTimeOffset.UtcNow;

        // Assert
        role.UpdatedAt.Should().NotBeNull();
    }
}
