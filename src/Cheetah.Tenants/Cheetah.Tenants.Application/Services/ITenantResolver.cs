using Microsoft.AspNetCore.Http;

namespace Cheetah.Tenants.Application.Services;

public interface ITenantResolver
{
    Task<Guid?> ResolveTenantIdAsync(HttpContext httpContext);
}
