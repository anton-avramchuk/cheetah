using System.Security.Claims;

namespace Cheetah.Backend.IdentityCore.Domain;

public class IdentityUserClaim : IdentityClaim
{
    /// <summary>
    /// Gets or sets the primary key of the user associated with this claim.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected IdentityUserClaim()
    {
    }

    /// <summary>
    /// Internal constructor for domain logic
    /// </summary>
    internal IdentityUserClaim(Guid userId, Claim claim) : base(claim)
    {
        UserId = userId;
    }

    /// <summary>
    /// Public constructor for creating user claims
    /// </summary>
    public IdentityUserClaim(Guid userId, string claimType, string claimValue)
        : base(claimType, claimValue)
    {
        UserId = userId;
    }
}
