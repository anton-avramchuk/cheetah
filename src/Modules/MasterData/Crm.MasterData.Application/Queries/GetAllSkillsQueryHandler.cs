using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllSkillsQuery, GridResult<SkillModel>>))]
public class GetAllSkillsQueryHandler(IGridRepository<Skill> repository)
    : IQueryHandler<GetAllSkillsQuery, GridResult<SkillModel>>
{
    public async ValueTask<GridResult<SkillModel>> HandleAsync(GetAllSkillsQuery gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<SkillModel>(gridRequest, ct);
    }
}
