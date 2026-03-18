using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetLocationsGridQuery, GridResult<LocationModel>>))]
public class GetLocationsGridQueryHandler(IGridRepository<Location> repository)
    : IQueryHandler<GetLocationsGridQuery, GridResult<LocationModel>>
{
    public async ValueTask<GridResult<LocationModel>> HandleAsync(GetLocationsGridQuery gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<LocationModel>(gridRequest, ct);
    }
}
