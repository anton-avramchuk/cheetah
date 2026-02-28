using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllWorkFormatsQuery, GridResult<WorkFormatModel>>))]
public class GetAllWorkFormatsQueryHandler(IGridRepository<WorkFormat> repository)
    : IQueryHandler<GetAllWorkFormatsQuery, GridResult<WorkFormatModel>>
{
    public async ValueTask<GridResult<WorkFormatModel>> HandleAsync(GetAllWorkFormatsQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<WorkFormatModel>(gridRequest, ct);
    }
}
