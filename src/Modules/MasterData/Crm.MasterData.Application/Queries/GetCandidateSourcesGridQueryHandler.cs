using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCandidateSourcesGridQuery, GridResult<CandidateSourceModel>>))]
public class GetCandidateSourcesGridQueryHandler(IGridRepository<CandidateSource> repository)
    : IQueryHandler<GetCandidateSourcesGridQuery, GridResult<CandidateSourceModel>>
{
    public async ValueTask<GridResult<CandidateSourceModel>> HandleAsync(GetCandidateSourcesGridQuery gridQuery, CancellationToken ct = default)
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
