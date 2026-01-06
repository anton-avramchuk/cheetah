using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.Domain;
using Cheetah.Identity.Events;
using EmailVO = Cheetah.Backend.IdentityCore.Domain.ValueObjects.Email;

namespace Cheetah.Identity.Domain.Entities;

/// <summary>
/// User aggregate root
/// NOTE: In tenant-per-database architecture, User is stored in tenant-specific DB
/// User existence in a tenant DB = membership in that tenant (no need for UserTenant junction)
/// </summary>
public class User : IdentityUser<Role>, ICreateAtEntity, IUpdatedAtEntity
{
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User() { } // For EF Core

    private User(string email, string? firstName = null, string? lastName = null)
        : base(email, email) // Use email as both username and email
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        IsActive = true;
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    public static User Create(
        string email,
        string passwordHash,
        string? firstName = null,
        string? lastName = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        // Validate email format using Email value object
        var emailVO = EmailVO.Create(email);

        var user = new User(emailVO.Value, firstName, lastName);
        user.ChangePasswordHash(passwordHash);

        user.AddDomainEvent(new UserCreatedEvent(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            DateTime.UtcNow
        ));

        return user;
    }

    /// <summary>
    /// Assigns roles to user
    /// In tenant-per-database, roles are managed within tenant context
    /// </summary>
    public void AssignRoles(List<Role> roles)
    {
        // Use base class method to clear and add roles
        foreach (var role in Roles.ToList())
        {
            base.RemoveRole(role.Role);
        }

        foreach (var role in roles)
        {
            base.AddRole(role);
        }
    }

    /// <summary>
    /// Records user login
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;

        AddDomainEvent(new UserLoggedInEvent(
            Id,
            Email,
            LastLoginAt.Value
        ));
    }

    /// <summary>
    /// Changes user password
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        base.ChangePasswordHash(newPasswordHash);

        AddDomainEvent(new PasswordChangedEvent(
            Id,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Confirms user email address
    /// </summary>
    public void ConfirmEmail()
    {
        if (EmailConfirmed)
            return; // Already confirmed

        base.ChangeEmailConfirmed(true);

        AddDomainEvent(new EmailConfirmedEvent(
            Id,
            Email,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Deactivates user account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activates user account
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Updates user profile
    /// </summary>
    public void UpdateProfile(string? firstName, string? lastName)
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
    }

    /// <summary>
    /// Add a personal permission to user
    /// </summary>
    public void AddPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission cannot be empty", nameof(permission));

        // Check if permission already exists
        if (Claims.Any(c => c.ClaimType == "Permission" && c.ClaimValue == permission))
            return; // Already has this permission

        base.AddClaim(new System.Security.Claims.Claim("Permission", permission));
    }

    /// <summary>
    /// Remove a personal permission from user
    /// </summary>
    public void RemovePermission(string permission)
    {
        base.RemoveClaim(new System.Security.Claims.Claim("Permission", permission));
    }

    /// <summary>
    /// Add a custom claim to user
    /// </summary>
    public void AddClaim(string claimType, string claimValue)
    {
        if (string.IsNullOrWhiteSpace(claimType))
            throw new ArgumentException("Claim type cannot be empty", nameof(claimType));

        if (string.IsNullOrWhiteSpace(claimValue))
            throw new ArgumentException("Claim value cannot be empty", nameof(claimValue));

        // Check if claim already exists
        if (Claims.Any(c => c.ClaimType == claimType && c.ClaimValue == claimValue))
            return;

        base.AddClaim(new System.Security.Claims.Claim(claimType, claimValue));
    }

    /// <summary>
    /// Remove a claim from user
    /// </summary>
    public void RemoveClaim(string claimType, string claimValue)
    {
        base.RemoveClaim(new System.Security.Claims.Claim(claimType, claimValue));
    }

    /// <summary>
    /// Get all personal permissions for this user (does NOT include role permissions)
    /// </summary>
    public IEnumerable<string> GetPersonalPermissions()
    {
        return Claims
            .Where(c => c.ClaimType == "Permission")
            .Select(c => c.ClaimValue);
    }

    /// <summary>
    /// Check if user has a specific personal permission (does NOT check role permissions)
    /// </summary>
    public bool HasPersonalPermission(string permission)
    {
        return Claims.Any(c => c.ClaimType == "Permission" && c.ClaimValue == permission);
    }
}
