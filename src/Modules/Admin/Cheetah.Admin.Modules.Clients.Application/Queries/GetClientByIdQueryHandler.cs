using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetClientByIdQuery, ClientModel?>))]
public class GetClientByIdQueryHandler : IQueryHandler<GetClientByIdQuery, ClientModel?>
{
    private readonly IClientRepository _repository;

    public GetClientByIdQueryHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<ClientModel?> HandleAsync(GetClientByIdQuery query, CancellationToken ct = default)
    {
        var client = await _repository.GetByIdAsync(query.Id, ct);
        if (client is null)
            return null;

        return new ClientModel(
            client.Id,
            client.Name,
            client.Description,
            client.TenantId.HasValue ? new TenantModel(client.TenantId.Value, "Default") : null);
    }
}
