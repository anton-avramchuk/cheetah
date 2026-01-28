using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllClientsQuery, IReadOnlyList<ClientModel>>))]
public class GetAllClientsQueryHandler : IQueryHandler<GetAllClientsQuery, IReadOnlyList<ClientModel>>
{
    private readonly IClientRepository _repository;

    public GetAllClientsQueryHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<ClientModel>> HandleAsync(GetAllClientsQuery query, CancellationToken ct = default)
    {
        var clients = await _repository.GetAllAsync(spec: null, ct);

        return clients
            .Select(c => new ClientModel(
                c.Id,
                c.Name,
                new TenantModel(c.TenantId ?? Guid.Empty, "Default")))
            .ToList();
    }
}
