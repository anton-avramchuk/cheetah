using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.Domain;

namespace Cheetah.Identity.Domain.Entities;

/// <summary>
/// User claim - represents a personal permission or attribute assigned to a user
/// Example: ClaimType = "Permission", ClaimValue = "Reports.ViewAll"
/// </summary>
public class UserClaim : IdentityUserClaim
{
    

    private UserClaim(Guid userId, string claimType, string claimValue)
        : base(userId, claimType, claimValue)
    {
    }

    /// <summary>
    /// Create a permission claim for a user
    /// </summary>
    public static UserClaim CreatePermission(Guid userId, string permission)
    {
        return new UserClaim(userId, "Permission", permission)
        {
            Id = Guid.NewGuid()
        };
    }

    /// <summary>
    /// Create a custom claim for a user
    /// </summary>
    public static UserClaim Create(Guid userId, string claimType, string claimValue)
    {
        return new UserClaim(userId, claimType, claimValue)
        {
            Id = Guid.NewGuid()
        };
    }
}
