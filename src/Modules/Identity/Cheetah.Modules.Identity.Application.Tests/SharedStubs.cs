using Cheetah.Modules.Identity.Domain;

namespace Cheetah.Modules.Identity.Application.Tests;

public sealed class StubRole : IdentityRole
{
    public StubRole() : base(Guid.NewGuid(), "stub") { }
    public StubRole(Guid id, string name) : base(id, name) { }
}

public sealed class StubUser : IdentityUser<StubRole>
{
    public StubUser(string userName, string email) : base(Guid.NewGuid(), userName, email) { }
}
