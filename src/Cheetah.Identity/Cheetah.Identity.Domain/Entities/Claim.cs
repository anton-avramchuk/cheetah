using Cheetah.Core.Domain;

namespace Cheetah.Identity.Domain.Entities;

/// <summary>
/// Base class for claims (Role claims, User claims)
/// Represents a permission or attribute
/// </summary>
public abstract class Claim : Entity<Guid>
{
    public string ClaimType { get; protected set; } = null!;
    public string ClaimValue { get; protected set; } = null!;

    protected Claim() { } // For EF Core

    protected Claim(string claimType, string claimValue)
    {
        if (string.IsNullOrWhiteSpace(claimType))
            throw new ArgumentException("Claim type cannot be empty", nameof(claimType));

        if (string.IsNullOrWhiteSpace(claimValue))
            throw new ArgumentException("Claim value cannot be empty", nameof(claimValue));

        ClaimType = claimType.Trim();
        ClaimValue = claimValue.Trim();
    }

    /// <summary>
    /// Convert to System.Security.Claims.Claim
    /// </summary>
    public virtual System.Security.Claims.Claim ToClaim()
    {
        return new System.Security.Claims.Claim(ClaimType, ClaimValue);
    }
}
