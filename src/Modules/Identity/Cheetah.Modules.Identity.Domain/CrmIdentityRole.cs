
namespace Cheetah.Modules.Identity.Domain;

public class CrmIdentityRole : IdentityRole
{
    private CrmIdentityRole() { } // For EF Core

    private CrmIdentityRole(Guid id, string name) : base(id, name) { }

    public new static CrmIdentityRole Create(string name) => new(Guid.NewGuid(), name);

    public new static CrmIdentityRole Create(Guid id, string name) => new(id, name);
}
