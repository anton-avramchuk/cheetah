using AppName.Identity.Domain;
using Shouldly;

namespace AppName.Identity.Domain.Tests;

public class AppNameIdentityRoleTests
{
    [Fact]
    public void Create_WithValidName_ShouldSetNameAndNormalizedName()
    {
        var role = AppNameIdentityRole.Create("admin");

        role.ShouldNotBeNull();
        role.Id.ShouldNotBe(Guid.Empty);
        role.Name.ShouldBe("admin");
        role.NormalizedName.ShouldBe("ADMIN");
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var role1 = AppNameIdentityRole.Create("role1");
        var role2 = AppNameIdentityRole.Create("role2");

        role1.Id.ShouldNotBe(role2.Id);
    }

    [Fact]
    public void Create_WithSpecificId_ShouldUseProvidedId()
    {
        var id = Guid.NewGuid();
        var role = AppNameIdentityRole.Create(id, "admin");

        role.Id.ShouldBe(id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        Should.Throw<ArgumentException>(() => AppNameIdentityRole.Create(name!));
    }

    [Fact]
    public void ChangeName_WithValidName_ShouldUpdateNameAndNormalizedName()
    {
        var role = AppNameIdentityRole.Create("old-name");

        role.ChangeName("new-name");

        role.Name.ShouldBe("new-name");
        role.NormalizedName.ShouldBe("NEW-NAME");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeName_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var role = AppNameIdentityRole.Create("admin");

        Should.Throw<ArgumentException>(() => role.ChangeName(name!));
    }
}
