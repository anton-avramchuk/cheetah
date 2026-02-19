using System.Security.Claims;

namespace Cheetah.Core.Identity.Domain;

public class IdentityUserClaim : IdentityClaim
{
    public Guid UserId { get; private set; }

    private IdentityUserClaim() { } // For EF Core

    private IdentityUserClaim(Guid userId, string claimType, string claimValue)
        : base(claimType, claimValue)
    {
        UserId = userId;
    }

    internal static IdentityUserClaim Create(Guid userId, Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        return new IdentityUserClaim(userId, claim.Type, claim.Value);
    }

    internal static IdentityUserClaim Create(Guid userId, string claimType, string claimValue)
        => new(userId, claimType, claimValue);
}
