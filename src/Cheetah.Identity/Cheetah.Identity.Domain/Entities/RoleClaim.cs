using Cheetah.Core.Domain;

namespace Cheetah.Identity.Domain.Entities;

/// <summary>
/// Role claim - represents a permission assigned to a role
/// Example: ClaimType = "Permission", ClaimValue = "Users.Create"
/// </summary>
public class RoleClaim : Claim
{
    public Guid RoleId { get; private set; }

    private RoleClaim() { } // For EF Core

    private RoleClaim(Guid roleId, string claimType, string claimValue)
        : base(claimType, claimValue)
    {
        RoleId = roleId;
    }

    /// <summary>
    /// Create a permission claim for a role
    /// </summary>
    public static RoleClaim CreatePermission(Guid roleId, string permission)
    {
        return new RoleClaim(roleId, "Permission", permission)
        {
            Id = Guid.NewGuid()
        };
    }

    /// <summary>
    /// Create a custom claim for a role
    /// </summary>
    public static RoleClaim Create(Guid roleId, string claimType, string claimValue)
    {
        return new RoleClaim(roleId, claimType, claimValue)
        {
            Id = Guid.NewGuid()
        };
    }
}
