using Cheetah.Core.Domain;

namespace Cheetah.Identity.Domain.Entities;

/// <summary>
/// Junction entity for many-to-many relationship between User and Role (from Permissions module)
/// TenantId is NOT needed here because UserRole is stored in tenant-specific DB (tenant-per-database)
/// </summary>
public class UserRole : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    private UserRole() { } // For EF Core

    public static UserRole Create(Guid userId, Guid roleId)
    {
        return new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };
    }
}
