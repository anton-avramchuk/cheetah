using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCandidateSourcesQuery, GridResult<CandidateSourceModel>>))]
public class GetAllCandidateSourcesQueryHandler(IGridRepository<CandidateSource> repository)
    : IQueryHandler<GetAllCandidateSourcesQuery, GridResult<CandidateSourceModel>>
{
    public async ValueTask<GridResult<CandidateSourceModel>> HandleAsync(GetAllCandidateSourcesQuery gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<CandidateSourceModel>(gridRequest, ct);
    }
}
