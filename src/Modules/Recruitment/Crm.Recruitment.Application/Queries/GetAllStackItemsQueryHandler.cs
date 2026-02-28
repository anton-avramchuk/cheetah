using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllStackItemsQuery, GridResult<StackItemModel>>))]
public class GetAllStackItemsQueryHandler(IGridRepository<StackItem> repository)
    : IQueryHandler<GetAllStackItemsQuery, GridResult<StackItemModel>>
{
    public async ValueTask<GridResult<StackItemModel>> HandleAsync(GetAllStackItemsQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<StackItemModel>(gridRequest, ct);
    }
}
