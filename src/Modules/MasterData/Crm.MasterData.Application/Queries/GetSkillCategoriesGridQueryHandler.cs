using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetSkillCategoriesGridQuery, GridResult<SkillCategoryModel>>))]
public class GetSkillCategoriesGridQueryHandler(IGridRepository<SkillCategory> repository)
    : IQueryHandler<GetSkillCategoriesGridQuery, GridResult<SkillCategoryModel>>
{
    public async ValueTask<GridResult<SkillCategoryModel>> HandleAsync(GetSkillCategoriesGridQuery gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<SkillCategoryModel>(gridRequest, ct);
    }
}
