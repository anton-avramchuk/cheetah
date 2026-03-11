using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.MasterData.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.MasterData.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Skill>), typeof(IRepository<Skill, Guid>))]
public class SkillRepository : EfGridRepository<MasterDataDbContext, Skill>
{
    public SkillRepository(MasterDataDbContext context, IObjectMapper mapper, ILogger<SkillRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
