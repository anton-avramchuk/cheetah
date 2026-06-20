using System.Security.Claims;
using Cheetah.Core.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Cheetah.FeatureManagement.AspNetCore;

/// <summary>
/// Строит <see cref="FeatureContext"/> из HTTP-запроса для декларативного <c>RequireFeature</c>.
/// Приложение может заменить реализацию (свои claim-имена / источник тенанта).
/// </summary>
public interface IHttpFeatureContextBuilder
{
    FeatureContext Build(HttpContext httpContext);
}

/// <summary>
/// Реализация по умолчанию: UserId из <see cref="ClaimTypes.NameIdentifier"/>,
/// TenantId из claim <c>tenant_id</c>, роли из role-claims.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IHttpFeatureContextBuilder))]
public sealed class DefaultHttpFeatureContextBuilder : IHttpFeatureContextBuilder
{
    public FeatureContext Build(HttpContext httpContext)
    {
        var user = httpContext.User;
        if (user?.Identity is null || !user.Identity.IsAuthenticated)
            return FeatureContext.Empty;

        Guid? userId = Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var u) ? u : null;
        Guid? tenantId = Guid.TryParse(user.FindFirstValue("tenant_id"), out var t) ? t : null;
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

        return new FeatureContext { UserId = userId, TenantId = tenantId, Roles = roles };
    }
}
