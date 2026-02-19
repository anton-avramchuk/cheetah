using Cheetah.Core.Domain;

namespace Cheetah.Core.Identity.Domain;

public class IdentityUserRole<TIdentityRole> : Entity
    where TIdentityRole : IdentityRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public TIdentityRole Role { get; private set; } = null!;

    private IdentityUserRole() { } // For EF Core

    internal IdentityUserRole(Guid userId, TIdentityRole role)
    {
        ArgumentNullException.ThrowIfNull(role);
        UserId = userId;
        Role = role;
        RoleId = role.Id;
    }

    public override object?[] GetKeys() => [UserId, RoleId];
}
