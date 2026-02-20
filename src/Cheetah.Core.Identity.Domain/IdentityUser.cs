using System.Security.Claims;
using Cheetah.Core.Domain;

namespace Cheetah.Core.Identity.Domain;

public class IdentityUser<TIdentityRole> : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
    where TIdentityRole : IdentityRole
{
    public string UserName { get; private set; } = null!;
    public string NormalizedUserName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string NormalizedEmail { get; private set; } = null!;
    public bool EmailConfirmed { get; private set; }
    public string? PasswordHash { get; private set; }
    public string SecurityStamp { get; private set; } = null!;
    public DateTimeOffset? LockoutEnd { get; private set; }
    public bool LockoutEnabled { get; private set; }
    public int AccessFailedCount { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private readonly List<IdentityUserRole<TIdentityRole>> _roles = new();
    public IReadOnlyCollection<IdentityUserRole<TIdentityRole>> Roles => _roles;

    private readonly List<IdentityUserClaim> _claims = new();
    public IReadOnlyCollection<IdentityUserClaim> Claims => _claims;

    private IdentityUser() { } // For EF Core

    private IdentityUser(Guid id, string userName, string email) : base(id)
    {
        SetUserName(userName);
        SetEmail(email);
        SecurityStamp = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Creates a new user. Concrete modules extending this class should declare their own
    /// static <c>Create()</c> method (hiding this one with <c>new</c>) and raise
    /// the appropriate domain event, e.g. <c>UserCreatedEvent</c>.
    /// </summary>
    public static IdentityUser<TIdentityRole> Create(string userName, string email)
        => new(Guid.NewGuid(), userName, email);

    // User name

    public void ChangeUserName(string userName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        SetUserName(userName);
    }

    private void SetUserName(string userName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        UserName = userName;
        NormalizedUserName = userName.ToUpperInvariant();
    }

    // Email

    public void ChangeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        SetEmail(email);
        EmailConfirmed = false;
    }

    private void SetEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
    }

    public void ConfirmEmail() => EmailConfirmed = true;

    public void SetEmailConfirmed(bool confirmed) => EmailConfirmed = confirmed;

    // Password & security

    public void SetPasswordHash(string? passwordHash) => PasswordHash = passwordHash;

    public void RefreshSecurityStamp() => SecurityStamp = Guid.NewGuid().ToString();

    public void SetSecurityStamp(string stamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stamp);
        SecurityStamp = stamp;
    }

    // Lockout

    public void SetLockoutEnd(DateTimeOffset? lockoutEnd) => LockoutEnd = lockoutEnd;

    public void SetLockoutEnabled(bool enabled) => LockoutEnabled = enabled;

    public void IncrementAccessFailedCount() => AccessFailedCount++;

    public void ResetAccessFailedCount() => AccessFailedCount = 0;

    // Roles

    public void AddRole(TIdentityRole role)
    {
        ArgumentNullException.ThrowIfNull(role);
        if (IsInRole(role.Id)) return;
        _roles.Add(new IdentityUserRole<TIdentityRole>(Id, role));
    }

    public void RemoveRole(TIdentityRole role)
    {
        ArgumentNullException.ThrowIfNull(role);
        _roles.RemoveAll(r => r.RoleId == role.Id);
    }

    public bool IsInRole(Guid roleId) => _roles.Any(r => r.RoleId == roleId);

    public bool IsInRole(TIdentityRole role)
    {
        ArgumentNullException.ThrowIfNull(role);
        return IsInRole(role.Id);
    }

    // Claims

    public void AddClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        _claims.Add(IdentityUserClaim.Create(Id, claim));
    }

    public void AddClaims(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);
        foreach (var claim in claims)
            AddClaim(claim);
    }

    public IdentityUserClaim? FindClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        return _claims.FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);
    }

    public void ReplaceClaim(Claim claim, Claim newClaim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(newClaim);
        foreach (var userClaim in _claims.Where(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value))
            userClaim.SetClaim(newClaim);
    }

    public void RemoveClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        _claims.RemoveAll(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);
    }

    public void RemoveClaims(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);
        foreach (var claim in claims)
            RemoveClaim(claim);
    }

    public override string ToString() => $"{base.ToString()}, UserName = {UserName}";
}
