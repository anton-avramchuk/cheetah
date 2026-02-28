using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllTaskPrioritiesQuery, GridResult<TaskPriorityModel>>))]
public class GetAllTaskPrioritiesQueryHandler(IGridRepository<TaskPriority> repository)
    : IQueryHandler<GetAllTaskPrioritiesQuery, GridResult<TaskPriorityModel>>
{
    public async ValueTask<GridResult<TaskPriorityModel>> HandleAsync(GetAllTaskPrioritiesQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<TaskPriorityModel>(gridRequest, ct);
    }
}
