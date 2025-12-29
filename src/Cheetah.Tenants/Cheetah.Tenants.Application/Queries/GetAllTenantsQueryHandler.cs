using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllTenantsQuery, IReadOnlyList<Tenant>>))]
public class GetAllTenantsQueryHandler(
    IReadOnlyRepository<Tenant, Guid> repository
) : IQueryHandler<GetAllTenantsQuery, IReadOnlyList<Tenant>>
{
    public async ValueTask<IReadOnlyList<Tenant>> HandleAsync(GetAllTenantsQuery query, CancellationToken cancellationToken = default)
    {
        return await repository.GetAllAsync(cancellationToken);
    }
}
