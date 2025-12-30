using System.Security.Claims;
using Cheetah.Core.Modularity;
using Microsoft.AspNetCore.Http;

namespace Cheetah.Identity.Application.Services;

/// <summary>
/// Current user implementation using HttpContext
/// </summary>
[Export(LifetimeType.Scoped, typeof(ICurrentUser))]
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPermissionService _permissionService;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor,
        IPermissionService permissionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _permissionService = permissionService;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Email
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Email)?.Value;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirst("TenantId")?.Value;

            return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    public async Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default)
    {
        if (!UserId.HasValue)
            return false;

        return await _permissionService.HasPermissionAsync(UserId.Value, permission, ct);
    }

    public async Task RequirePermissionAsync(string permission, CancellationToken ct = default)
    {
        if (!UserId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated");

        await _permissionService.RequirePermissionAsync(UserId.Value, permission, ct);
    }
}
