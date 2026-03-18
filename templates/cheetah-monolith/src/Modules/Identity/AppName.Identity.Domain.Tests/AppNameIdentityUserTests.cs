using AppName.Identity.Domain;
using Shouldly;

namespace AppName.Identity.Domain.Tests;

public class AppNameIdentityUserTests
{
    [Fact]
    public void Create_WithValidUserNameAndEmail_ShouldSetProperties()
    {
        var user = AppNameIdentityUser.Create("johndoe", "john@example.com");

        user.ShouldNotBeNull();
        user.Id.ShouldNotBe(Guid.Empty);
        user.UserName.ShouldBe("johndoe");
        user.NormalizedUserName.ShouldBe("JOHNDOE");
        user.Email.ShouldBe("john@example.com");
        user.NormalizedEmail.ShouldBe("JOHN@EXAMPLE.COM");
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var user1 = AppNameIdentityUser.Create("user1", "user1@example.com");
        var user2 = AppNameIdentityUser.Create("user2", "user2@example.com");

        user1.Id.ShouldNotBe(user2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidUserName_ShouldThrowArgumentException(string? userName)
    {
        Should.Throw<ArgumentException>(() => AppNameIdentityUser.Create(userName!, "email@example.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string? email)
    {
        Should.Throw<ArgumentException>(() => AppNameIdentityUser.Create("johndoe", email!));
    }

    [Fact]
    public void ChangeUserName_WithValidName_ShouldUpdateUserNameAndNormalizedName()
    {
        var user = AppNameIdentityUser.Create("oldname", "john@example.com");

        user.ChangeUserName("newname");

        user.UserName.ShouldBe("newname");
        user.NormalizedUserName.ShouldBe("NEWNAME");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeUserName_WithInvalidName_ShouldThrowArgumentException(string? userName)
    {
        var user = AppNameIdentityUser.Create("johndoe", "john@example.com");

        Should.Throw<ArgumentException>(() => user.ChangeUserName(userName!));
    }

    [Fact]
    public void ChangeEmail_WithValidEmail_ShouldUpdateEmailAndNormalizedEmail()
    {
        var user = AppNameIdentityUser.Create("johndoe", "old@example.com");

        user.ChangeEmail("new@example.com");

        user.Email.ShouldBe("new@example.com");
        user.NormalizedEmail.ShouldBe("NEW@EXAMPLE.COM");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeEmail_WithInvalidEmail_ShouldThrowArgumentException(string? email)
    {
        var user = AppNameIdentityUser.Create("johndoe", "john@example.com");

        Should.Throw<ArgumentException>(() => user.ChangeEmail(email!));
    }

    [Fact]
    public void RefreshSecurityStamp_ShouldChangeSecurityStamp()
    {
        var user = AppNameIdentityUser.Create("johndoe", "john@example.com");
        var originalStamp = user.SecurityStamp;

        user.RefreshSecurityStamp();

        user.SecurityStamp.ShouldNotBe(originalStamp);
    }
}
