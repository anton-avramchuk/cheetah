using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllSampleEntitiesQuery, GridResult<CandidateModel>>))]
public class GetAllSampleEntitiesQueryHandler(IGridRepository<Candidate> repository)
    : IQueryHandler<GetAllSampleEntitiesQuery, GridResult<CandidateModel>>
{
    public async ValueTask<GridResult<CandidateModel>> HandleAsync(GetAllSampleEntitiesQuery gridQuery,
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