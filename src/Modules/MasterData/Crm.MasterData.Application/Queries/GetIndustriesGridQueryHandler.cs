using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetIndustriesGridQuery, GridResult<IndustryModel>>))]
public class GetIndustriesGridQueryHandler(IGridRepository<Industry> repository)
    : IQueryHandler<GetIndustriesGridQuery, GridResult<IndustryModel>>
{
    public async ValueTask<GridResult<IndustryModel>> HandleAsync(GetIndustriesGridQuery gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<IndustryModel>(gridRequest, ct);
    }
}
