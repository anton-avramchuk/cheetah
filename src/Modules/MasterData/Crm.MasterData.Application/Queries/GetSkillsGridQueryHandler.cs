using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetSkillsGridQuery, GridResult<SkillModel>>))]
public class GetSkillsGridQueryHandler(IGridRepository<Skill> repository)
    : IQueryHandler<GetSkillsGridQuery, GridResult<SkillModel>>
{
    public async ValueTask<GridResult<SkillModel>> HandleAsync(GetSkillsGridQuery gridQuery, CancellationToken ct = default)
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
