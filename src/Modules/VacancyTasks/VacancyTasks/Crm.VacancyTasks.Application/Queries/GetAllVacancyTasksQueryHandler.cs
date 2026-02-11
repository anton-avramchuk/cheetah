using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllVacancyTasksQuery, GridResult<VacancyTaskModel>>))]
public class GetAllVacancyTasksQueryHandler(IGridRepository<VacancyTask> repository)
    : IQueryHandler<GetAllVacancyTasksQuery, GridResult<VacancyTaskModel>>
{
    public async ValueTask<GridResult<VacancyTaskModel>> HandleAsync(GetAllVacancyTasksQuery gridQuery,
        CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<VacancyTaskModel>(gridRequest, ct);
    }
}
