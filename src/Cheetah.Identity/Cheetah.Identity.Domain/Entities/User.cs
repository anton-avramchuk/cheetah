using Cheetah.Core.Domain;
using Cheetah.Identity.Domain.ValueObjects;
using Cheetah.Identity.Events;

namespace Cheetah.Identity.Domain.Entities;

/// <summary>
/// User aggregate root
/// NOTE: In tenant-per-database architecture, User is stored in tenant-specific DB
/// User existence in a tenant DB = membership in that tenant (no need for UserTenant junction)
/// </summary>
public class User : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Email { get; private set; } = null!;
    public string NormalizedEmail { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private readonly List<UserRole> _roles = [];
    public IReadOnlyList<UserRole> Roles => _roles.AsReadOnly();

    private User() { } // For EF Core

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

        // Validate email format
        var emailVO = ValueObjects.Email.Create(email);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = emailVO.Value,
            NormalizedEmail = emailVO.Normalize(),
            PasswordHash = passwordHash,
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim(),
            IsActive = true,
            EmailConfirmed = false
        };

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
    public void AssignRoles(List<Guid> roleIds)
    {
        // Clear existing roles
        _roles.Clear();

        // Add new roles
        foreach (var roleId in roleIds)
        {
            var userRole = UserRole.Create(Id, roleId);
            _roles.Add(userRole);
        }
    }

    /// <summary>
    /// Adds a single role to user
    /// </summary>
    public void AddRole(Guid roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId))
            return; // Already has this role

        var userRole = UserRole.Create(Id, roleId);
        _roles.Add(userRole);
    }

    /// <summary>
    /// Removes a role from user
    /// </summary>
    public void RemoveRole(Guid roleId)
    {
        var role = _roles.FirstOrDefault(r => r.RoleId == roleId);
        if (role != null)
        {
            _roles.Remove(role);
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
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;

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

        EmailConfirmed = true;

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
}
