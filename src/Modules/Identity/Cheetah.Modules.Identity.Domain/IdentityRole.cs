using System.Security.Claims;
using Cheetah.Core.Domain;

namespace Cheetah.Modules.Identity.Domain;

public class IdentityRole : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private readonly List<IdentityRoleClaim> _claims = new();
    public IReadOnlyCollection<IdentityRoleClaim> Claims => _claims;

    protected IdentityRole() { } // For EF Core

    protected IdentityRole(Guid id, string name) : base(id)
    {
        SetName(name);
    }

    public static IdentityRole Create(string name)
        => new(Guid.NewGuid(), name);

    public static IdentityRole Create(Guid id, string name)
        => new(id, name);

    public void ChangeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        SetName(name);
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }

    public void AddClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        _claims.Add(IdentityRoleClaim.Create(Id, claim));
    }

    public void RemoveClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        _claims.RemoveAll(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);
    }

    public override string ToString() => $"{base.ToString()}, Name = {Name}";
}
