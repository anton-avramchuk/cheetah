using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCandidatesQuery, GridResult<CandidateModel>>))]
public class GetAllCandidatesQueryHandler(IGridRepository<Candidate> repository)
    : IQueryHandler<GetAllCandidatesQuery, GridResult<CandidateModel>>
{
    public async ValueTask<GridResult<CandidateModel>> HandleAsync(GetAllCandidatesQuery gridQuery,
        CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<CandidateModel>(gridRequest, ct);
    }
}
