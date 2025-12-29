using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTenantByIdQuery, Tenant?>))]
public class GetTenantByIdQueryHandler(
    IReadOnlyRepository<Tenant, Guid> repository
) : IQueryHandler<GetTenantByIdQuery, Tenant?>
{
    public async ValueTask<Tenant?> HandleAsync(GetTenantByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await repository.GetAsync(query.TenantId, cancellationToken);
    }
}
