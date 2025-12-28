using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Cheetah.Tenants.Application.Services;

[Export(LifetimeType.Scoped)]
public class TenantResolver(IDispatcher dispatcher) : ITenantResolver
{
    private const string TenantIdHeaderName = "X-Tenant-Id";
    private const string TenantSubdomainHeaderName = "X-Tenant-Subdomain";

    public async Task<Guid?> ResolveTenantIdAsync(HttpContext httpContext)
    {
        // Try to resolve from X-Tenant-Id header
        if (httpContext.Request.Headers.TryGetValue(TenantIdHeaderName, out var tenantIdValue) &&
            Guid.TryParse(tenantIdValue, out var tenantId))
        {
            return tenantId;
        }

        // Try to resolve from X-Tenant-Subdomain header
        if (httpContext.Request.Headers.TryGetValue(TenantSubdomainHeaderName, out var subdomain) &&
            !string.IsNullOrWhiteSpace(subdomain))
        {
            var tenant = await dispatcher.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(new GetTenantBySubdomainQuery(subdomain!));
            if (tenant != null)
            {
                return tenant.Id;
            }
        }

        // Try to resolve from subdomain in Host header
        var host = httpContext.Request.Host.Host;
        var parts = host.Split('.');

        if (parts.Length >= 3) // e.g., tenant1.mycrm.com
        {
            var potentialSubdomain = parts[0];
            var tenant = await dispatcher.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(new GetTenantBySubdomainQuery(potentialSubdomain));
            if (tenant != null)
            {
                return tenant.Id;
            }
        }

        return null;
    }
}
