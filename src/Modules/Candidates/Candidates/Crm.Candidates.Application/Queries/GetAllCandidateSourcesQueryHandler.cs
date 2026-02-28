using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCandidateSourcesQuery, GridResult<CandidateSourceModel>>))]
public class GetAllCandidateSourcesQueryHandler(IGridRepository<CandidateSource> repository)
    : IQueryHandler<GetAllCandidateSourcesQuery, GridResult<CandidateSourceModel>>
{
    public async ValueTask<GridResult<CandidateSourceModel>> HandleAsync(GetAllCandidateSourcesQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<CandidateSourceModel>(gridRequest, ct);
    }
}
