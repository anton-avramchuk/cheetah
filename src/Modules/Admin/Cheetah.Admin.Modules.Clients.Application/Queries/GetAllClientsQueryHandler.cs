using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllClientsQuery, GridResult<ClientModel>>))]
public class GetAllClientsQueryHandler : IQueryHandler<GetAllClientsQuery, GridResult<ClientModel>>
{
    private readonly IGridRepository<Client> _repository;

    public GetAllClientsQueryHandler(IGridRepository<Client> repository)
    {
        _repository = repository;
    }

    public async ValueTask<GridResult<ClientModel>> HandleAsync(GetAllClientsQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };

        return await _repository.GetGridAsync<ClientModel>(gridRequest, ct);
    }
}
