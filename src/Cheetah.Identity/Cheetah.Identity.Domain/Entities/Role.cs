using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.Domain;
using Cheetah.Identity.Events;

namespace Cheetah.Identity.Domain.Entities;

/// <summary>
/// Role aggregate root
/// Represents a role with permissions (via RoleClaims)
/// </summary>
public class Role : IdentityRole, ICreateAtEntity, IUpdatedAtEntity
{
    public string? Description { get; private set; }

    private Role() { } // For EF Core

    private Role(string name, string? description = null) : base(name)
    {
        Description = description?.Trim();
    }

    /// <summary>
    /// Creates a new role
    /// </summary>
    public static Role Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        var role = new Role(name.Trim(), description);

        role.AddDomainEvent(new RoleCreatedEvent(
            role.Id,
            role.Name,
            DateTime.UtcNow
        ));

        return role;
    }

    /// <summary>
    /// Add a permission to this role
    /// </summary>
    public void AddPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission cannot be empty", nameof(permission));

        // Check if permission already exists
        if (_claims.Any(c => c.ClaimType == "Permission" && c.ClaimValue == permission))
            return; // Already has this permission

        var roleClaim = RoleClaim.CreatePermission(Id, permission);
        _claims.Add(roleClaim);

        AddDomainEvent(new RoleClaimAddedEvent(
            Id,
            Name,
            "Permission",
            permission,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Remove a permission from this role
    /// </summary>
    public void RemovePermission(string permission)
    {
        var claim = _claims.FirstOrDefault(c => c.ClaimType == "Permission" && c.ClaimValue == permission);
        if (claim != null)
        {
            _claims.Remove(claim);
        }
    }

    /// <summary>
    /// Add a custom claim to this role
    /// </summary>
    public void AddClaim(string claimType, string claimValue)
    {
        if (string.IsNullOrWhiteSpace(claimType))
            throw new ArgumentException("Claim type cannot be empty", nameof(claimType));

        if (string.IsNullOrWhiteSpace(claimValue))
            throw new ArgumentException("Claim value cannot be empty", nameof(claimValue));

        // Check if claim already exists
        if (_claims.Any(c => c.ClaimType == claimType && c.ClaimValue == claimValue))
            return;

        var roleClaim = RoleClaim.Create(Id, claimType, claimValue);
        _claims.Add(roleClaim);

        AddDomainEvent(new RoleClaimAddedEvent(
            Id,
            Name,
            claimType,
            claimValue,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Remove a claim from this role
    /// </summary>
    public void RemoveClaim(string claimType, string claimValue)
    {
        var claim = _claims.FirstOrDefault(c => c.ClaimType == claimType && c.ClaimValue == claimValue);
        if (claim != null)
        {
            _claims.Remove(claim);
        }
    }

    /// <summary>
    /// Update role information
    /// </summary>
    public void Update(string name, string? description = null)
    {
        ChangeName(name.Trim());
        Description = description?.Trim();
    }

    /// <summary>
    /// Get all permissions for this role
    /// </summary>
    public IEnumerable<string> GetPermissions()
    {
        return _claims
            .Where(c => c.ClaimType == "Permission")
            .Select(c => c.ClaimValue);
    }

    /// <summary>
    /// Check if role has a specific permission
    /// </summary>
    public bool HasPermission(string permission)
    {
        return _claims.Any(c => c.ClaimType == "Permission" && c.ClaimValue == permission);
    }
}
