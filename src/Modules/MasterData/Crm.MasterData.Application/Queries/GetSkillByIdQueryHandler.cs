using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.MasterData.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetSkillByIdQuery, SkillModel?>))]
public class GetSkillByIdQueryHandler(IRepository<Skill, Guid> repository)
    : IQueryHandler<GetSkillByIdQuery, SkillModel?>
{
    public async ValueTask<SkillModel?> HandleAsync(GetSkillByIdQuery query, CancellationToken ct = default)
    {
        var entity = await repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new SkillModel(entity.Id, entity.Name, entity.SkillCategoryId);
    }
}
