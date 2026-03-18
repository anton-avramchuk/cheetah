using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetWorkFormatsGridQuery, GridResult<WorkFormatModel>>))]
public class GetWorkFormatsGridQueryHandler(IGridRepository<WorkFormat> repository)
    : IQueryHandler<GetWorkFormatsGridQuery, GridResult<WorkFormatModel>>
{
    public async ValueTask<GridResult<WorkFormatModel>> HandleAsync(GetWorkFormatsGridQuery gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<WorkFormatModel>(gridRequest, ct);
    }
}
