using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllVacancyRolesQuery, GridResult<VacancyRoleModel>>))]
public class GetAllVacancyRolesQueryHandler(IGridRepository<VacancyRole> repository)
    : IQueryHandler<GetAllVacancyRolesQuery, GridResult<VacancyRoleModel>>
{
    public async ValueTask<GridResult<VacancyRoleModel>> HandleAsync(GetAllVacancyRolesQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<VacancyRoleModel>(gridRequest, ct);
    }
}
