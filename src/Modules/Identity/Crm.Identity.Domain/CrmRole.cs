using Cheetah.Core.Identity.Domain;

namespace Crm.Identity.Domain;

public class CrmRole : IdentityRole
{
    private CrmRole() { } // For EF Core

    private CrmRole(Guid id, string name) : base(id, name) { }

    public new static CrmRole Create(string name) => new(Guid.NewGuid(), name);

    public new static CrmRole Create(Guid id, string name) => new(id, name);
}
