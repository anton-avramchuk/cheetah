using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllClientsQuery, IReadOnlyList<ClientModel>>))]
public class GetAllClientsQueryHandler : IQueryHandler<GetAllClientsQuery, IReadOnlyList<ClientModel>>
{
    public ValueTask<IReadOnlyList<ClientModel>> HandleAsync(GetAllClientsQuery query, CancellationToken ct = default)
    {
        // TODO: Replace with repository when Domain layer is implemented
        var result = Enumerable.Range(1, 30)
            .Select((x, i) => new ClientModel(
                Guid.NewGuid(),
                $"Client {i}",
                new TenantModel(Guid.NewGuid(), $"Tenant {i}")))
            .ToList();

        return ValueTask.FromResult<IReadOnlyList<ClientModel>>(result);
    }
}