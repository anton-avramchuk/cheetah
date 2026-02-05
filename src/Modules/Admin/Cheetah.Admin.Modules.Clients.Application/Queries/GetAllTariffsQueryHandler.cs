using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllTariffsQuery, GridResult<TariffModel>>))]
public class GetAllTariffsQueryHandler : IQueryHandler<GetAllTariffsQuery, GridResult<TariffModel>>
{
    private readonly ITariffRepository _repository;
    private readonly IGridQueryService _gridService;

    public GetAllTariffsQueryHandler(ITariffRepository repository, IGridQueryService gridService)
    {
        _repository = repository;
        _gridService = gridService;
    }

    public async ValueTask<GridResult<TariffModel>> HandleAsync(GetAllTariffsQuery query, CancellationToken ct = default)
    {
        var queryable = _repository.AsNoTrackingQueryable();

        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };

        return await _gridService.ExecuteAsync<Tariff, TariffModel>(queryable, gridRequest, ct);
    }
}
