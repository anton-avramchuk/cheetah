using System.Security.Claims;
using Cheetah.Core.Domain;

namespace Cheetah.Modules.Identity.Domain;

public abstract class IdentityClaim : Entity<Guid>
{
    public string ClaimType { get; private set; } = null!;
    public string ClaimValue { get; private set; } = null!;

    protected IdentityClaim() { } // For EF Core

    protected IdentityClaim(string claimType, string claimValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(claimType);
        ArgumentException.ThrowIfNullOrWhiteSpace(claimValue);
        Id = Guid.NewGuid();
        ClaimType = claimType;
        ClaimValue = claimValue;
    }

    public Claim ToClaim() => new(ClaimType, ClaimValue);

    internal void SetClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        ClaimType = claim.Type;
        ClaimValue = claim.Value;
    }
}
