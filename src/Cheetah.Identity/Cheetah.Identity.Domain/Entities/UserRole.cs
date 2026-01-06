using Cheetah.Backend.IdentityCore.Domain;

namespace Cheetah.Identity.Domain.Entities;

/// <summary>
/// Junction entity for many-to-many relationship between User and Role (from Permissions module)
/// TenantId is NOT needed here because UserRole is stored in tenant-specific DB (tenant-per-database)
/// </summary>
public class UserRole : IdentityUserRole<Role>
{
    public DateTime AssignedAt { get; private set; }

    private UserRole() { } // For EF Core

    private UserRole(Guid userId, Role role) : base(userId, role)
    {
        AssignedAt = DateTime.UtcNow;
    }

    public static UserRole Create(Guid userId, Role role)
    {
        return new UserRole(userId, role);
    }
}
