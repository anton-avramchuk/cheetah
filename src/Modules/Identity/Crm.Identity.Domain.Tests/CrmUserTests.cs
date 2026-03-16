using Cheetah.Modules.Identity.Domain;
using Shouldly;

namespace Crm.Identity.Domain.Tests;

public class CrmUserTests
{
    [Fact]
    public void Create_WithValidUserNameAndEmail_ShouldSetProperties()
    {
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");

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
        var user1 = CrmIdentityUser.Create("user1", "user1@example.com");
        var user2 = CrmIdentityUser.Create("user2", "user2@example.com");

        user1.Id.ShouldNotBe(user2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidUserName_ShouldThrowArgumentException(string? userName)
    {
        Should.Throw<ArgumentException>(() => CrmIdentityUser.Create(userName!, "email@example.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string? email)
    {
        Should.Throw<ArgumentException>(() => CrmIdentityUser.Create("johndoe", email!));
    }

    [Fact]
    public void ChangeUserName_WithValidName_ShouldUpdateUserNameAndNormalizedName()
    {
        var user = CrmIdentityUser.Create("oldname", "john@example.com");

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
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");

        Should.Throw<ArgumentException>(() => user.ChangeUserName(userName!));
    }

    [Fact]
    public void ChangeEmail_WithValidEmail_ShouldUpdateEmailAndNormalizedEmail()
    {
        var user = CrmIdentityUser.Create("johndoe", "old@example.com");

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
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");

        Should.Throw<ArgumentException>(() => user.ChangeEmail(email!));
    }

    [Fact]
    public void RefreshSecurityStamp_ShouldChangeSecurityStamp()
    {
        var user = CrmIdentityUser.Create("johndoe", "john@example.com");
        var originalStamp = user.SecurityStamp;

        user.RefreshSecurityStamp();

        user.SecurityStamp.ShouldNotBe(originalStamp);
    }
}
