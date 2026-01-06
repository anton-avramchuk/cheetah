using System.Security.Claims;

namespace Cheetah.Backend.IdentityCore.Domain;

public class IdentityRoleClaim : IdentityClaim
{
    /// <summary>
    /// Gets or sets the primary key of the role associated with this claim.
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected IdentityRoleClaim()
    {
    }

    /// <summary>
    /// Internal constructor for domain logic
    /// </summary>
    protected internal IdentityRoleClaim(Guid roleId, Claim claim) : base(claim)
    {
        RoleId = roleId;
    }

    /// <summary>
    /// Public constructor for creating role claims
    /// </summary>
    protected internal IdentityRoleClaim(Guid roleId, string claimType, string claimValue)
        : base(claimType, claimValue)
    {
        RoleId = roleId;
    }
}
