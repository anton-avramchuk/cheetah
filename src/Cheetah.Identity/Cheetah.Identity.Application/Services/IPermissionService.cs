namespace Cheetah.Identity.Application.Services;

/// <summary>
/// Service for checking user permissions (Role Claims + User Claims)
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Check if user has a specific permission
    /// Combines Role permissions + User personal permissions
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct = default);

    /// <summary>
    /// Get all permissions for a user
    /// Combines Role permissions + User personal permissions
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Require permission (throws exception if user doesn't have it)
    /// </summary>
    Task RequirePermissionAsync(Guid userId, string permission, CancellationToken ct = default);
}
