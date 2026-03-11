using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllPositionsQuery, GridResult<PositionModel>>))]
public class GetAllPositionsQueryHandler(IGridRepository<Position> repository)
    : IQueryHandler<GetAllPositionsQuery, GridResult<PositionModel>>
{
    public async ValueTask<GridResult<PositionModel>> HandleAsync(GetAllPositionsQuery gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<PositionModel>(gridRequest, ct);
    }
}
