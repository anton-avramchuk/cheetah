using System.Security.Claims;
using Cheetah.Core.Domain;

namespace Cheetah.Backend.IdentityCore.Domain;

public class IdentityRole : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private readonly List<IdentityRoleClaim> _claims = new();
    public IReadOnlyCollection<IdentityRoleClaim> Claims => _claims;

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected IdentityRole()
    {
    }

    /// <summary>
    /// Private constructor for domain logic
    /// </summary>
    private IdentityRole(string name) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }

    /// <summary>
    /// Private constructor for domain logic with specific ID
    /// </summary>
    private IdentityRole(Guid id, string name) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }

    /// <summary>
    /// Factory method to create a new role
    /// </summary>
    public static IdentityRole Create(string name)
    {
        var role = new IdentityRole(name);
        role.AddDomainEvent(new Backend.IdentityCore.Events.RoleCreatedEvent(
            role.Id,
            name
        ));
        return role;
    }

    /// <summary>
    /// Factory method to create a new role with specific ID
    /// </summary>
    public static IdentityRole Create(Guid id, string name)
    {
        var role = new IdentityRole(id, name);
        role.AddDomainEvent(new Backend.IdentityCore.Events.RoleCreatedEvent(
            role.Id,
            name
        ));
        return role;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        var oldName = Name;
        Name = name;
        NormalizedName = name.ToUpperInvariant();

        AddDomainEvent(new Backend.IdentityCore.Events.RoleNameChangedEvent(
            Id,
            name,
            oldName
        ));
    }

    public void AddClaim(Claim claim)
    {
        if (claim == null)
            throw new ArgumentNullException(nameof(claim));

        _claims.Add(new IdentityRoleClaim(Id, claim));

        AddDomainEvent(new Backend.IdentityCore.Events.RoleClaimAddedEvent(
            Id,
            claim.Type,
            claim.Value
        ));
    }

    public void RemoveClaim(Claim claim)
    {
        if (claim == null)
            throw new ArgumentNullException(nameof(claim));

        _claims.RemoveAll(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);

        AddDomainEvent(new Backend.IdentityCore.Events.RoleClaimRemovedEvent(
            Id,
            claim.Type,
            claim.Value
        ));
    }

    public override string ToString()
    {
        return $"{base.ToString()}, Name = {Name}";
    }
}
