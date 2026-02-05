using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllClientsQuery, GridResult<ClientModel>>))]
public class GetAllClientsQueryHandler : IQueryHandler<GetAllClientsQuery, GridResult<ClientModel>>
{
    private readonly IClientRepository _repository;
    private readonly IGridQueryService _gridService;

    public GetAllClientsQueryHandler(IClientRepository repository, IGridQueryService gridService)
    {
        _repository = repository;
        _gridService = gridService;
    }

    public async ValueTask<GridResult<ClientModel>> HandleAsync(GetAllClientsQuery query, CancellationToken ct = default)
    {
        var queryable = _repository.AsNoTrackingQueryable();

        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };

        return await _gridService.ExecuteAsync<Client, ClientModel>(queryable, gridRequest, ct);
    }
}
