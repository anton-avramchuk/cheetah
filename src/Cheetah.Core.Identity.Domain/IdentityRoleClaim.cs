using System.Security.Claims;

namespace Cheetah.Core.Identity.Domain;

public class IdentityRoleClaim : IdentityClaim
{
    public Guid RoleId { get; private set; }

    private IdentityRoleClaim() { } // For EF Core

    private IdentityRoleClaim(Guid roleId, string claimType, string claimValue)
        : base(claimType, claimValue)
    {
        RoleId = roleId;
    }

    internal static IdentityRoleClaim Create(Guid roleId, Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        return new IdentityRoleClaim(roleId, claim.Type, claim.Value);
    }

    internal static IdentityRoleClaim Create(Guid roleId, string claimType, string claimValue)
        => new(roleId, claimType, claimValue);
}
