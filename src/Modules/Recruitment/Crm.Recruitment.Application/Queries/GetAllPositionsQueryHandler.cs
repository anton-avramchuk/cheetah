using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllPositionsQuery, GridResult<PositionModel>>))]
public class GetAllPositionsQueryHandler(IGridRepository<Position> repository)
    : IQueryHandler<GetAllPositionsQuery, GridResult<PositionModel>>
{
    public async ValueTask<GridResult<PositionModel>> HandleAsync(GetAllPositionsQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<PositionModel>(gridRequest, ct);
    }
}
