using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Application.Queries;

namespace Cheetah.Identity.Application.Services;

/// <summary>
/// Permission service implementation
/// Uses GetUserPermissionsQuery to get all user permissions (Role + Personal)
/// </summary>
[Export(LifetimeType.Scoped, typeof(IPermissionService))]
public class PermissionService : IPermissionService
{
    private readonly IDispatcher _dispatcher;

    public PermissionService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        var permissions = await GetUserPermissionsAsync(userId, ct);
        return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        var query = new GetUserPermissionsQuery(userId);
        return await _dispatcher.QueryAsync<GetUserPermissionsQuery, List<string>>(query, ct);
    }

    public async Task RequirePermissionAsync(Guid userId, string permission, CancellationToken ct = default)
    {
        var hasPermission = await HasPermissionAsync(userId, permission, ct);
        if (!hasPermission)
        {
            throw new UnauthorizedAccessException($"User {userId} does not have permission '{permission}'");
        }
    }
}
