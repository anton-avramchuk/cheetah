using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.MasterData.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.MasterData.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<SkillCategory>), typeof(IRepository<SkillCategory, Guid>))]
public class SkillCategoryRepository : EfGridRepository<MasterDataDbContext, SkillCategory>
{
    public SkillCategoryRepository(MasterDataDbContext context, IObjectMapper mapper, ILogger<SkillCategoryRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
