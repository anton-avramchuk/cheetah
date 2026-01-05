using Cheetah.Core.Domain;

namespace Cheetah.Backend.IdentityCore.Domain;

public class IdentityUserRole<TIdentityRole> : Entity<Guid> where TIdentityRole : IdentityRole
{
    /// <summary>
    /// Gets or sets the primary key of the user that is linked to a role.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets or sets the primary key of the role that is linked to the user.
    /// </summary>
    public Guid RoleId { get; private set; }

    public TIdentityRole Role { get; private set; } = null!;

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    private IdentityUserRole()
    {
    }

    /// <summary>
    /// Constructor for creating a new user-role relationship
    /// </summary>
    public IdentityUserRole(Guid userId, TIdentityRole role) : base(Guid.NewGuid())
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        UserId = userId;
        SetRole(role);
    }

    public void SetRole(TIdentityRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        Role = role;
        RoleId = role.Id;
    }
}
