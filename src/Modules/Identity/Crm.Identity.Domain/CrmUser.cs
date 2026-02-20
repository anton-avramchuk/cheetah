using Cheetah.Core.Identity.Domain;

namespace Crm.Identity.Domain;

public class CrmUser : IdentityUser<CrmRole>
{
    private CrmUser() { } // For EF Core

    private CrmUser(Guid id, string userName, string email) : base(id, userName, email) { }

    public new static CrmUser Create(string userName, string email) => new(Guid.NewGuid(), userName, email);
}
