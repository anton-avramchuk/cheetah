using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllVacancyStatesQuery, GridResult<VacancyStateModel>>))]
public class GetAllVacancyStatesQueryHandler(IGridRepository<VacancyState> repository)
    : IQueryHandler<GetAllVacancyStatesQuery, GridResult<VacancyStateModel>>
{
    public async ValueTask<GridResult<VacancyStateModel>> HandleAsync(GetAllVacancyStatesQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<VacancyStateModel>(gridRequest, ct);
    }
}
