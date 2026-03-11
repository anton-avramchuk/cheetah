using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetSkillCategoryByIdQuery, SkillCategoryModel?>))]
public class GetSkillCategoryByIdQueryHandler(IRepository<SkillCategory, Guid> repository)
    : IQueryHandler<GetSkillCategoryByIdQuery, SkillCategoryModel?>
{
    public async ValueTask<SkillCategoryModel?> HandleAsync(GetSkillCategoryByIdQuery query, CancellationToken ct = default)
    {
        var entity = await repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new SkillCategoryModel(entity.Id, entity.Name);
    }
}
