using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Queries;

[Export(LifetimeType.Scoped)]
public class GetTenantBySubdomainQueryHandler(
    IReadOnlyRepository<Tenant, Guid> repository
) : IQueryHandler<GetTenantBySubdomainQuery, Tenant?>
{
    public async ValueTask<Tenant?> HandleAsync(GetTenantBySubdomainQuery query, CancellationToken cancellationToken = default)
    {
        var subdomain = query.Subdomain.ToLowerInvariant();
        return await repository.FirstOrDefaultAsync(t => t.Subdomain == subdomain, cancellationToken);
    }
}
