using AppName.Domain;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;

namespace AppName.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetSampleEntitiesGridQuery, GridResult<SampleEntityModel>>))]
public class GetSampleEntitiesGridQueryHandler(IGridRepository<SampleEntity> repository)
    : IQueryHandler<GetSampleEntitiesGridQuery, GridResult<SampleEntityModel>>
{
    public async ValueTask<GridResult<SampleEntityModel>> HandleAsync(GetSampleEntitiesGridQuery gridQuery,
        CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<SampleEntityModel>(gridRequest, ct);
    }
}
