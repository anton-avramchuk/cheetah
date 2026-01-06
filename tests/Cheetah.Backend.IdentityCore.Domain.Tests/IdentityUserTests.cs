using System.Security.Claims;
using Cheetah.Backend.IdentityCore.Domain;
using FluentAssertions;

namespace Cheetah.Backend.IdentityCore.Domain.Tests;

public class IdentityUserTests
{
    [Fact]
    public void Create_WithValidCredentials_ShouldCreateUser()
    {
        // Arrange
        var userName = "testuser";
        var email = "test@example.com";

        // Act
        var user = IdentityUser<TestIdentityRole>.Create(userName, email);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBe(Guid.Empty);
        user.UserName.Should().Be(userName);
        user.NormalizedUserName.Should().Be(userName.ToUpperInvariant());
        user.Email.Should().Be(email);
        user.NormalizedEmail.Should().Be(email.ToUpperInvariant());
        user.SecurityStamp.Should().NotBeNullOrEmpty();
        user.EmailConfirmed.Should().BeFalse();
        user.LockoutEnabled.Should().BeTrue();
        user.AccessFailedCount.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyUserName_ShouldThrowArgumentException(string? invalidUserName)
    {
        // Act
        Action act = () => IdentityUser<TestIdentityRole>.Create(invalidUserName!, "test@example.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*User name cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act
        Action act = () => IdentityUser<TestIdentityRole>.Create("testuser", invalidEmail!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void ChangeUserName_WithValidUserName_ShouldUpdateUserName()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var newUserName = "newusername";

        // Act
        user.ChangeUserName(newUserName);

        // Assert
        user.UserName.Should().Be(newUserName);
        user.NormalizedUserName.Should().Be(newUserName.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ChangeUserName_WithEmptyUserName_ShouldThrowArgumentException(string? invalidUserName)
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");

        // Act
        Action act = () => user.ChangeUserName(invalidUserName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*User name cannot be empty*");
    }

    [Fact]
    public void ChangeEmail_WithValidEmail_ShouldUpdateEmail()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var newEmail = "newemail@example.com";

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        user.Email.Should().Be(newEmail);
        user.NormalizedEmail.Should().Be(newEmail.ToUpperInvariant());
    }

    [Fact]
    public void ChangeEmail_ShouldRaiseDomainEvent()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var newEmail = "newemail@example.com";
        user.ClearDomainEvents();

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.UserEmailChangedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ChangeEmail_WithEmptyEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");

        // Act
        Action act = () => user.ChangeEmail(invalidEmail!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void ChangePasswordHash_WithValidHash_ShouldUpdatePasswordAndSecurityStamp()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var oldSecurityStamp = user.SecurityStamp;
        var newPasswordHash = "new-hashed-password";

        // Act
        user.ChangePasswordHash(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
        user.SecurityStamp.Should().NotBe(oldSecurityStamp);
    }

    [Fact]
    public void ChangePasswordHash_ShouldRaiseDomainEvent()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        user.ClearDomainEvents();

        // Act
        user.ChangePasswordHash("new-hashed-password");

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.UserPasswordChangedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ChangePasswordHash_WithEmptyHash_ShouldThrowArgumentException(string? invalidHash)
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");

        // Act
        Action act = () => user.ChangePasswordHash(invalidHash!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password hash cannot be empty*");
    }

    [Fact]
    public void ChangeEmailConfirmed_ShouldUpdateEmailConfirmed()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");

        // Act
        user.ChangeEmailConfirmed(true);

        // Assert
        user.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void LockUser_WithFutureDate_ShouldLockUser()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var lockoutEnd = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        user.LockUser(lockoutEnd);

        // Assert
        user.LockoutEnd.Should().Be(lockoutEnd);
    }

    [Fact]
    public void LockUser_ShouldRaiseDomainEvent()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var lockoutEnd = DateTimeOffset.UtcNow.AddHours(1);
        user.ClearDomainEvents();

        // Act
        user.LockUser(lockoutEnd);

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.UserLockedOutEvent>();
    }

    [Fact]
    public void LockUser_WithPastDate_ShouldThrowArgumentException()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var lockoutEnd = DateTimeOffset.UtcNow.AddHours(-1);

        // Act
        Action act = () => user.LockUser(lockoutEnd);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Lockout end must be in the future*");
    }

    [Fact]
    public void UnlockUser_ShouldClearLockoutAndResetAccessFailedCount()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        user.LockUser(DateTimeOffset.UtcNow.AddHours(1));
        user.IncrementAccessFailedCount();

        // Act
        user.UnlockUser();

        // Assert
        user.LockoutEnd.Should().BeNull();
        user.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public void UnlockUser_ShouldRaiseDomainEvent()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        user.LockUser(DateTimeOffset.UtcNow.AddHours(1));
        user.ClearDomainEvents();

        // Act
        user.UnlockUser();

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.UserUnlockedEvent>();
    }

    [Fact]
    public void IncrementAccessFailedCount_ShouldIncreaseCount()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var initialCount = user.AccessFailedCount;

        // Act
        user.IncrementAccessFailedCount();

        // Assert
        user.AccessFailedCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void ResetAccessFailedCount_ShouldResetToZero()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        user.IncrementAccessFailedCount();
        user.IncrementAccessFailedCount();

        // Act
        user.ResetAccessFailedCount();

        // Assert
        user.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public void AddRole_WithValidRole_ShouldAddRoleToUser()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var role = TestIdentityRole.Create("Admin");

        // Act
        user.AddRole(role);

        // Assert
        user.Roles.Should().HaveCount(1);
        var userRole = user.Roles.First();
        userRole.RoleId.Should().Be(role.Id);
        userRole.Role.Should().Be(role);
    }

    [Fact]
    public void AddRole_WithDuplicateRole_ShouldNotAddAgain()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var role = TestIdentityRole.Create("Admin");
        user.AddRole(role);

        // Act
        user.AddRole(role);

        // Assert
        user.Roles.Should().HaveCount(1);
    }

    [Fact]
    public void AddRole_ShouldRaiseDomainEvent()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var role = TestIdentityRole.Create("Admin");
        user.ClearDomainEvents();

        // Act
        user.AddRole(role);

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.UserRoleAddedEvent>();
    }

    [Fact]
    public void AddRole_WithNullRole_ShouldThrowArgumentNullException()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");

        // Act
        Action act = () => user.AddRole(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveRole_WithExistingRole_ShouldRemoveRole()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var role = TestIdentityRole.Create("Admin");
        user.AddRole(role);

        // Act
        user.RemoveRole(role);

        // Assert
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_ShouldRaiseDomainEvent()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var role = TestIdentityRole.Create("Admin");
        user.AddRole(role);
        user.ClearDomainEvents();

        // Act
        user.RemoveRole(role);

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.UserRoleRemovedEvent>();
    }

    [Fact]
    public void IsInRole_WithAssignedRole_ShouldReturnTrue()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var role = TestIdentityRole.Create("Admin");
        user.AddRole(role);

        // Act
        var result = user.IsInRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInRole_WithNonAssignedRole_ShouldReturnFalse()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var role = TestIdentityRole.Create("Admin");

        // Act
        var result = user.IsInRole(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInRoleById_WithAssignedRoleId_ShouldReturnTrue()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var role = TestIdentityRole.Create("Admin");
        user.AddRole(role);

        // Act
        var result = user.IsInRole(role.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AddClaim_WithValidClaim_ShouldAddClaimToUser()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var claim = new Claim("Permission", "Users.Create");

        // Act
        user.AddClaim(claim);

        // Assert
        user.Claims.Should().HaveCount(1);
        var userClaim = user.Claims.First();
        userClaim.ClaimType.Should().Be("Permission");
        userClaim.ClaimValue.Should().Be("Users.Create");
    }

    [Fact]
    public void AddClaim_ShouldRaiseDomainEvent()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var claim = new Claim("Permission", "Users.Create");
        user.ClearDomainEvents();

        // Act
        user.AddClaim(claim);

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.UserClaimAddedEvent>();
    }

    [Fact]
    public void RemoveClaim_WithExistingClaim_ShouldRemoveClaim()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var claim = new Claim("Permission", "Users.Create");
        user.AddClaim(claim);

        // Act
        user.RemoveClaim(claim);

        // Assert
        user.Claims.Should().BeEmpty();
    }

    [Fact]
    public void RemoveClaim_ShouldRaiseDomainEvent()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var claim = new Claim("Permission", "Users.Create");
        user.AddClaim(claim);
        user.ClearDomainEvents();

        // Act
        user.RemoveClaim(claim);

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First();
        domainEvent.Should().BeOfType<Backend.IdentityCore.Events.UserClaimRemovedEvent>();
    }

    [Fact]
    public void FindClaim_WithExistingClaim_ShouldReturnClaim()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var claim = new Claim("Permission", "Users.Create");
        user.AddClaim(claim);

        // Act
        var foundClaim = user.FindClaim(claim);

        // Assert
        foundClaim.Should().NotBeNull();
        foundClaim!.ClaimType.Should().Be("Permission");
        foundClaim.ClaimValue.Should().Be("Users.Create");
    }

    [Fact]
    public void FindClaim_WithNonExistingClaim_ShouldReturnNull()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var claim = new Claim("Permission", "Users.Create");

        // Act
        var foundClaim = user.FindClaim(claim);

        // Assert
        foundClaim.Should().BeNull();
    }

    [Fact]
    public void ReplaceClaim_WithExistingClaim_ShouldReplaceClaim()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");
        var oldClaim = new Claim("Permission", "Users.Create");
        var newClaim = new Claim("Permission", "Users.Delete");
        user.AddClaim(oldClaim);

        // Act
        user.ReplaceClaim(oldClaim, newClaim);

        // Assert
        user.Claims.Should().HaveCount(1);
        var userClaim = user.Claims.First();
        userClaim.ClaimType.Should().Be("Permission");
        userClaim.ClaimValue.Should().Be("Users.Delete");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var user = IdentityUser<TestIdentityRole>.Create("testuser", "test@example.com");

        // Act
        var result = user.ToString();

        // Assert
        result.Should().Contain("testuser");
    }
}
