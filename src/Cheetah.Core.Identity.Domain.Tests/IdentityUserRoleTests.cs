namespace Cheetah.Core.Identity.Domain.Tests;

public class IdentityUserRoleTests
{
    // IdentityUserRole<T> is internal-constructed via IdentityUser.AddRole.
    // We test it through the aggregate's public API.

    private static IdentityUser<IdentityRole> CreateUser()
        => IdentityUser<IdentityRole>.Create("john", "john@example.com");

    [Fact]
    public void UserRole_HasCorrectUserId()
    {
        var user = CreateUser();
        var role = IdentityRole.Create("admin");

        user.AddRole(role);

        user.Roles.First().UserId.ShouldBe(user.Id);
    }

    [Fact]
    public void UserRole_HasCorrectRoleId()
    {
        var user = CreateUser();
        var role = IdentityRole.Create("admin");

        user.AddRole(role);

        user.Roles.First().RoleId.ShouldBe(role.Id);
    }

    [Fact]
    public void UserRole_HasRoleReference()
    {
        var user = CreateUser();
        var role = IdentityRole.Create("admin");

        user.AddRole(role);

        user.Roles.First().Role.ShouldBeSameAs(role);
    }

    [Fact]
    public void UserRole_GetKeys_ReturnsBothIds()
    {
        var user = CreateUser();
        var role = IdentityRole.Create("admin");
        user.AddRole(role);

        var userRole = user.Roles.First();
        var keys = userRole.GetKeys();

        keys.ShouldBe([user.Id, role.Id]);
    }
}
