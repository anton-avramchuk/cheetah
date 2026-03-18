namespace Cheetah.Core.Identity.Domain.Tests;

internal sealed class TestRole : IdentityRole
{
    private TestRole() { }
    private TestRole(Guid id, string name) : base(id, name) { }

    public static TestRole Create(string name) => new(Guid.NewGuid(), name);
    public static TestRole Create(Guid id, string name) => new(id, name);
}

internal sealed class TestUser : IdentityUser<TestRole>
{
    private TestUser() { }
    private TestUser(Guid id, string userName, string email) : base(id, userName, email) { }

    public static TestUser Create(string userName, string email) => new(Guid.NewGuid(), userName, email);
}
