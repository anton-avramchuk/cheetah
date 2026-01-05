using System.Security.Claims;
using Cheetah.Core.Domain;

namespace Cheetah.Backend.IdentityCore.Domain;

public abstract class IdentityClaim : Entity<Guid>
{
    /// <summary>
    /// Gets or sets the claim type for this claim.
    /// </summary>
    public string ClaimType { get; private set; } = null!;

    /// <summary>
    /// Gets or sets the claim value for this claim.
    /// </summary>
    public string ClaimValue { get; private set; } = null!;

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected IdentityClaim()
    {
    }

    /// <summary>
    /// Protected constructor for derived classes
    /// </summary>
    protected IdentityClaim(Claim claim) : this(claim.Type, claim.Value)
    {
    }

    /// <summary>
    /// Protected constructor for derived classes
    /// </summary>
    protected IdentityClaim(string claimType, string claimValue) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(claimType))
            throw new ArgumentException("Claim type cannot be empty", nameof(claimType));

        ClaimType = claimType;
        ClaimValue = claimValue ?? string.Empty;
    }

    public virtual Claim ToClaim()
    {
        return new Claim(ClaimType, ClaimValue);
    }

    public virtual void SetClaim(Claim claim)
    {
        if (claim == null)
            throw new ArgumentNullException(nameof(claim));
        if (string.IsNullOrWhiteSpace(claim.Type))
            throw new ArgumentException("Claim type cannot be empty", nameof(claim));

        ClaimType = claim.Type;
        ClaimValue = claim.Value ?? string.Empty;
    }
}
