using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCandidateApplicationsQuery, GridResult<CandidateApplicationModel>>))]
public class GetAllCandidateApplicationsQueryHandler(IGridRepository<CandidateApplication> repository)
    : IQueryHandler<GetAllCandidateApplicationsQuery, GridResult<CandidateApplicationModel>>
{
    public async ValueTask<GridResult<CandidateApplicationModel>> HandleAsync(GetAllCandidateApplicationsQuery gridQuery,
        CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<CandidateApplicationModel>(gridRequest, ct);
    }
}
