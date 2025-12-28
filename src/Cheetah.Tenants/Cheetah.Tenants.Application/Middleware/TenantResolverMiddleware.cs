using Cheetah.Tenants.Application.Services;
using Microsoft.AspNetCore.Http;

namespace Cheetah.Tenants.Application.Middleware;

public class TenantResolverMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver, ICurrentTenant currentTenant)
    {
        var tenantId = await tenantResolver.ResolveTenantIdAsync(context);

        if (tenantId.HasValue)
        {
            currentTenant.SetTenant(tenantId);
        }

        await next(context);
    }
}
