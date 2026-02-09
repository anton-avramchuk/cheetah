using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllTariffsQuery, GridResult<TariffModel>>))]
public class GetAllTariffsQueryHandler : IQueryHandler<GetAllTariffsQuery, GridResult<TariffModel>>
{
    private readonly IGridRepository<Tariff> _repository;

    public GetAllTariffsQueryHandler(IGridRepository<Tariff> repository)
    {
        _repository = repository;
    }

    public async ValueTask<GridResult<TariffModel>> HandleAsync(GetAllTariffsQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };

        return await _repository.GetGridAsync<TariffModel>(gridRequest, ct);
    }
}
