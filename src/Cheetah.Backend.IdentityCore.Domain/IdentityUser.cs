using System.Security.Claims;
using Cheetah.Core.Domain;

namespace Cheetah.Backend.IdentityCore.Domain;

public class IdentityUser<TIdentityRole> : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
    where TIdentityRole : IdentityRole
{
    public string UserName { get; private set; } = null!;

    public string NormalizedUserName { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public bool EmailConfirmed { get; private set; }

    public string NormalizedEmail { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

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

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected IdentityUser()
    {
    }

    /// <summary>
    /// Private constructor for domain logic
    /// </summary>
    private IdentityUser(string userName, string email) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be empty", nameof(userName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        UserName = userName;
        NormalizedUserName = userName.ToUpperInvariant();
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
        SecurityStamp = Guid.NewGuid().ToString();
        PasswordHash = string.Empty;
        EmailConfirmed = false;
        LockoutEnabled = true;
        AccessFailedCount = 0;
    }

    /// <summary>
    /// Factory method to create a new user
    /// </summary>
    public static IdentityUser<TIdentityRole> Create(string userName, string email)
    {
        var user = new IdentityUser<TIdentityRole>(userName, email);
        user.AddDomainEvent(new Backend.IdentityCore.Events.UserCreatedEvent(
            user.Id,
            userName,
            email
        ));
        return user;
    }

    public void ChangeUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be empty", nameof(userName));

        UserName = userName;
        NormalizedUserName = userName.ToUpperInvariant();
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        var oldEmail = Email;
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();

        AddDomainEvent(new Backend.IdentityCore.Events.UserEmailChangedEvent(
            Id,
            email,
            oldEmail
        ));
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        PasswordHash = passwordHash;
        SecurityStamp = Guid.NewGuid().ToString();

        AddDomainEvent(new Backend.IdentityCore.Events.UserPasswordChangedEvent(Id));
    }

    public void ChangeEmailConfirmed(bool emailConfirmed)
    {
        EmailConfirmed = emailConfirmed;
    }

    public void ChangeSecurityStamp(string securityStamp)
    {
        if (string.IsNullOrWhiteSpace(securityStamp))
            throw new ArgumentException("Security stamp cannot be empty", nameof(securityStamp));

        SecurityStamp = securityStamp;
    }

    public void LockUser(DateTimeOffset lockoutEnd)
    {
        if (lockoutEnd <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Lockout end must be in the future", nameof(lockoutEnd));

        LockoutEnd = lockoutEnd;

        AddDomainEvent(new Backend.IdentityCore.Events.UserLockedOutEvent(Id, lockoutEnd));
    }

    public void UnlockUser()
    {
        LockoutEnd = null;
        AccessFailedCount = 0;

        AddDomainEvent(new Backend.IdentityCore.Events.UserUnlockedEvent(Id));
    }

    public void IncrementAccessFailedCount()
    {
        AccessFailedCount++;
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
    }

    public void AddRole(TIdentityRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (IsInRole(role.Id))
            return;

        _roles.Add(new IdentityUserRole<TIdentityRole>(Id, role));

        AddDomainEvent(new Backend.IdentityCore.Events.UserRoleAddedEvent(Id, role.Id));
    }

    public void RemoveRole(TIdentityRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (!IsInRole(role))
            return;

        _roles.RemoveAll(r => r.RoleId == role.Id);

        AddDomainEvent(new Backend.IdentityCore.Events.UserRoleRemovedEvent(Id, role.Id));
    }

    public bool IsInRole(Guid roleId)
    {
        return Roles.Any(r => r.RoleId == roleId);
    }

    public bool IsInRole(TIdentityRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        return Roles.Any(r => r.RoleId == role.Id);
    }

    public void AddClaim(Claim claim)
    {
        if (claim == null)
            throw new ArgumentNullException(nameof(claim));

        _claims.Add(new IdentityUserClaim(Id, claim));

        AddDomainEvent(new Backend.IdentityCore.Events.UserClaimAddedEvent(
            Id,
            claim.Type,
            claim.Value
        ));
    }

    public void AddClaims(IEnumerable<Claim> claims)
    {
        if (claims == null)
            throw new ArgumentNullException(nameof(claims));

        foreach (var claim in claims)
        {
            AddClaim(claim);
        }
    }

    public IdentityUserClaim? FindClaim(Claim claim)
    {
        if (claim == null)
            throw new ArgumentNullException(nameof(claim));

        return Claims.FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);
    }

    public void ReplaceClaim(Claim claim, Claim newClaim)
    {
        if (claim == null)
            throw new ArgumentNullException(nameof(claim));
        if (newClaim == null)
            throw new ArgumentNullException(nameof(newClaim));

        var userClaims = Claims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList();

        foreach (var userClaim in userClaims)
        {
            userClaim.SetClaim(newClaim);
        }
    }

    public void RemoveClaims(IEnumerable<Claim> claims)
    {
        if (claims == null)
            throw new ArgumentNullException(nameof(claims));

        foreach (var claim in claims)
        {
            RemoveClaim(claim);
        }
    }

    public void RemoveClaim(Claim claim)
    {
        if (claim == null)
            throw new ArgumentNullException(nameof(claim));

        _claims.RemoveAll(c => c.ClaimValue == claim.Value && c.ClaimType == claim.Type);

        AddDomainEvent(new Backend.IdentityCore.Events.UserClaimRemovedEvent(
            Id,
            claim.Type,
            claim.Value
        ));
    }

    public override string ToString()
    {
        return $"{base.ToString()}, UserName = {UserName}";
    }
}
