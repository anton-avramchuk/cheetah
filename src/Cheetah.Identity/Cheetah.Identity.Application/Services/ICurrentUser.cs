namespace Cheetah.Identity.Application.Services;

/// <summary>
/// Service for accessing current authenticated user information
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Current user ID (null if not authenticated)
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Current user email (null if not authenticated)
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Is user authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Current tenant ID (null if not in tenant context)
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Check if current user has a specific permission
    /// </summary>
    Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default);

    /// <summary>
    /// Require permission (throws if user doesn't have it)
    /// </summary>
    Task RequirePermissionAsync(string permission, CancellationToken ct = default);
}
