using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Repositories;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetVacanciesGridQuery, GridResult<VacancyModel>>))]
public class GetVacanciesGridQueryHandler(IVacancyRepository repository, IGridQueryService gridService)
    : IQueryHandler<GetVacanciesGridQuery, GridResult<VacancyModel>>
{
    public async ValueTask<GridResult<VacancyModel>> HandleAsync(GetVacanciesGridQuery gridQuery,
        CancellationToken ct = default)
    {
        var queryable = repository.AsNoTrackingQueryable();

        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await gridService.ExecuteAsync<Vacancy, VacancyModel>(queryable, gridRequest, ct);
    }
}
