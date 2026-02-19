using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Identity.Domain;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllSampleEntitiesQuery, GridResult<UserIdentityModel>>))]
public class GetAllSampleEntitiesQueryHandler(IGridRepository<UserIdentity> repository)
    : IQueryHandler<GetAllSampleEntitiesQuery, GridResult<UserIdentityModel>>
{
    public async ValueTask<GridResult<UserIdentityModel>> HandleAsync(GetAllSampleEntitiesQuery gridQuery,
        CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<UserIdentityModel>(gridRequest, ct);
    }
}