using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllSampleEntitiesQuery, GridResult<StackItemModel>>))]
public class GetAllSampleEntitiesQueryHandler(IGridRepository<StackItem> repository)
    : IQueryHandler<GetAllSampleEntitiesQuery, GridResult<StackItemModel>>
{
    public async ValueTask<GridResult<StackItemModel>> HandleAsync(GetAllSampleEntitiesQuery gridQuery,
        CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<StackItemModel>(gridRequest, ct);
    }
}