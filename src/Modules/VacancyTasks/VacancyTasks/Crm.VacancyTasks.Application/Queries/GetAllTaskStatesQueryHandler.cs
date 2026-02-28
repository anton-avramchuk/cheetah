using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllTaskStatesQuery, GridResult<TaskStateModel>>))]
public class GetAllTaskStatesQueryHandler(IGridRepository<TaskState> repository)
    : IQueryHandler<GetAllTaskStatesQuery, GridResult<TaskStateModel>>
{
    public async ValueTask<GridResult<TaskStateModel>> HandleAsync(GetAllTaskStatesQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<TaskStateModel>(gridRequest, ct);
    }
}
