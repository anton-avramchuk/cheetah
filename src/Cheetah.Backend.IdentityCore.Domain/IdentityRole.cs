using System.Security.Claims;
using Cheetah.Core.Domain;

namespace Cheetah.Backend.IdentityCore.Domain;

public abstract class IdentityRole : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    protected readonly List<IdentityRoleClaim> _claims = new();
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
    protected internal IdentityRole(string name) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }

    /// <summary>
    /// Private constructor for domain logic with specific ID
    /// </summary>
    protected internal IdentityRole(Guid id, string name) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        Name = name;
        NormalizedName = name.ToUpperInvariant();
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
