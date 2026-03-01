using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Position>), typeof(IRepository<Position, Guid>))]
public class PositionRepository : EfGridRepository<RecruitmentDbContext, Position>
{
    public PositionRepository(RecruitmentDbContext context, IObjectMapper mapper, ILogger<PositionRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
