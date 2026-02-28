using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCandidateStagesQuery, GridResult<CandidateStageModel>>))]
public class GetAllCandidateStagesQueryHandler(IGridRepository<CandidateStage> repository)
    : IQueryHandler<GetAllCandidateStagesQuery, GridResult<CandidateStageModel>>
{
    public async ValueTask<GridResult<CandidateStageModel>> HandleAsync(GetAllCandidateStagesQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<CandidateStageModel>(gridRequest, ct);
    }
}
