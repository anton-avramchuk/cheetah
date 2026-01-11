using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Events;
using FluentAssertions;

namespace Cheetah.Identity.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidCredentials_ShouldCreateUser()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashed_password_123";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = User.Create(email, passwordHash, firstName, lastName);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be(email);
        user.NormalizedEmail.Should().Be(email.ToUpperInvariant());
        user.UserName.Should().Be(email); // Email is used as username
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.IsActive.Should().BeTrue();
        user.EmailConfirmed.Should().BeFalse();
        user.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseUserCreatedEvent()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashed_password_123";

        // Act
        var user = User.Create(email, passwordHash);

        // Assert
        user.DomainEvents.Should().Contain(e => e is UserCreatedEvent);
        var createdEvent = user.DomainEvents.OfType<UserCreatedEvent>().First();
        createdEvent.UserId.Should().Be(user.Id);
        createdEvent.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act
        Action act = () => User.Create(invalidEmail!, "password_hash");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyPasswordHash_ShouldThrowArgumentException(string? invalidHash)
    {
        // Act
        Action act = () => User.Create("test@example.com", invalidHash!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password hash cannot be empty*");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.com")]
    public void Create_WithInvalidEmailFormat_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act
        Action act = () => User.Create(invalidEmail, "password_hash");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConfirmEmail_WhenNotConfirmed_ShouldConfirmEmail()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        user.ClearDomainEvents();

        // Act
        user.ConfirmEmail();

        // Assert
        user.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void ConfirmEmail_ShouldRaiseEmailConfirmedEvent()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        user.ClearDomainEvents();

        // Act
        user.ConfirmEmail();

        // Assert
        user.DomainEvents.Should().Contain(e => e is EmailConfirmedEvent);
        var confirmedEvent = user.DomainEvents.OfType<EmailConfirmedEvent>().First();
        confirmedEvent.UserId.Should().Be(user.Id);
        confirmedEvent.Email.Should().Be(user.Email);
    }

    [Fact]
    public void ConfirmEmail_WhenAlreadyConfirmed_ShouldNotRaiseEvent()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        user.ConfirmEmail();
        user.ClearDomainEvents();

        // Act
        user.ConfirmEmail();

        // Assert
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangePassword_WithValidHash_ShouldUpdatePassword()
    {
        // Arrange
        var user = User.Create("test@example.com", "old_hash");
        var newPasswordHash = "new_hash";
        user.ClearDomainEvents();

        // Act
        user.ChangePassword(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
    }

    [Fact]
    public void ChangePassword_ShouldRaisePasswordChangedEvent()
    {
        // Arrange
        var user = User.Create("test@example.com", "old_hash");
        user.ClearDomainEvents();

        // Act
        user.ChangePassword("new_hash");

        // Assert
        user.DomainEvents.Should().Contain(e => e is PasswordChangedEvent);
        var passwordChangedEvent = user.DomainEvents.OfType<PasswordChangedEvent>().First();
        passwordChangedEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void RecordLogin_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        var beforeLogin = DateTime.UtcNow.AddSeconds(-1);

        // Act
        user.RecordLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeAfter(beforeLogin);
    }

    [Fact]
    public void RecordLogin_ShouldRaiseUserLoggedInEvent()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        user.ClearDomainEvents();

        // Act
        user.RecordLogin();

        // Assert
        user.DomainEvents.Should().Contain(e => e is UserLoggedInEvent);
        var loggedInEvent = user.DomainEvents.OfType<UserLoggedInEvent>().First();
        loggedInEvent.UserId.Should().Be(user.Id);
        loggedInEvent.Email.Should().Be(user.Email);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateNames()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash", "John", "Doe");
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        user.UpdateProfile(newFirstName, newLastName);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
    }

    [Fact]
    public void UpdateProfile_WithWhitespace_ShouldTrimNames()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");

        // Act
        user.UpdateProfile("  Jane  ", "  Smith  ");

        // Assert
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
    }

    [Fact]
    public void AddPermission_WithValidPermission_ShouldAddPermission()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        var permission = "Users.Create";

        // Act
        user.AddPermission(permission);

        // Assert
        user.Claims.Should().HaveCount(1);
        user.HasPersonalPermission(permission).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddPermission_WithEmptyPermission_ShouldThrowArgumentException(string? invalidPermission)
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");

        // Act
        Action act = () => user.AddPermission(invalidPermission!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Permission cannot be empty*");
    }

    [Fact]
    public void AddPermission_WithDuplicatePermission_ShouldNotAddAgain()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        var permission = "Users.Create";
        user.AddPermission(permission);

        // Act
        user.AddPermission(permission);

        // Assert
        user.Claims.Should().HaveCount(1);
    }

    [Fact]
    public void RemovePermission_WithExistingPermission_ShouldRemovePermission()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        var permission = "Users.Create";
        user.AddPermission(permission);

        // Act
        user.RemovePermission(permission);

        // Assert
        user.HasPersonalPermission(permission).Should().BeFalse();
    }

    [Fact]
    public void GetPersonalPermissions_ShouldReturnOnlyPermissionClaims()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        user.AddPermission("Users.Create");
        user.AddPermission("Users.Delete");
        user.AddClaim("CustomClaim", "CustomValue");

        // Act
        var permissions = user.GetPersonalPermissions().ToList();

        // Assert
        permissions.Should().HaveCount(2);
        permissions.Should().Contain("Users.Create");
        permissions.Should().Contain("Users.Delete");
        permissions.Should().NotContain("CustomValue");
    }

    [Fact]
    public void AddClaim_WithValidClaim_ShouldAddClaim()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        var claimType = "Department";
        var claimValue = "IT";

        // Act
        user.AddClaim(claimType, claimValue);

        // Assert
        user.Claims.Should().HaveCount(1);
        var claim = user.Claims.First();
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
        var user = User.Create("test@example.com", "hash");

        // Act
        Action act = () => user.AddClaim(invalidType!, value);

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
        var user = User.Create("test@example.com", "hash");

        // Act
        Action act = () => user.AddClaim(type, invalidValue!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Claim value cannot be empty*");
    }

    [Fact]
    public void AddClaim_WithDuplicateClaim_ShouldNotAddAgain()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        var claimType = "Department";
        var claimValue = "IT";
        user.AddClaim(claimType, claimValue);

        // Act
        user.AddClaim(claimType, claimValue);

        // Assert
        user.Claims.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveClaim_WithExistingClaim_ShouldRemoveClaim()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        var claimType = "Department";
        var claimValue = "IT";
        user.AddClaim(claimType, claimValue);

        // Act
        user.RemoveClaim(claimType, claimValue);

        // Assert
        user.Claims.Should().BeEmpty();
    }

    [Fact]
    public void AssignRoles_ShouldClearExistingAndAddNewRoles()
    {
        // Arrange
        var user = User.Create("test@example.com", "hash");
        var role1 = Role.Create("Admin");
        var role2 = Role.Create("User");
        var role3 = Role.Create("Manager");
        user.AddRole(role1);

        // Act
        user.AssignRoles(new List<Role> { role2, role3 });

        // Assert
        user.Roles.Should().HaveCount(2);
        user.Roles.Should().Contain(r => r.RoleId == role2.Id);
        user.Roles.Should().Contain(r => r.RoleId == role3.Id);
        user.Roles.Should().NotContain(r => r.RoleId == role1.Id);
    }
}
