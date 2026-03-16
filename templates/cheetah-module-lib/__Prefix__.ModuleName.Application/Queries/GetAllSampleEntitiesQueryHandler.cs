using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using __Prefix__.ModuleName.Domain;

namespace __Prefix__.ModuleName.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllSampleEntitiesQuery, GridResult<SampleEntityModel>>))]
public class GetAllSampleEntitiesQueryHandler(IGridRepository<SampleEntity> repository)
    : IQueryHandler<GetAllSampleEntitiesQuery, GridResult<SampleEntityModel>>
{
    public async ValueTask<GridResult<SampleEntityModel>> HandleAsync(GetAllSampleEntitiesQuery gridQuery,
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
