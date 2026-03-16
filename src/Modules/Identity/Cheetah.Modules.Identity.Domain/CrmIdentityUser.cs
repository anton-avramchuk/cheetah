using Cheetah.Core.Identity.Domain;

namespace Cheetah.Modules.Identity.Domain;

public class CrmIdentityUser : IdentityUser<CrmIdentityRole>
{
    private CrmIdentityUser() { } // For EF Core

    private CrmIdentityUser(Guid id, string userName, string email) : base(id, userName, email) { }

    public new static CrmIdentityUser Create(string userName, string email) => new(Guid.NewGuid(), userName, email);
}
