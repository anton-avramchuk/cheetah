using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.MasterData.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.MasterData.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Position>), typeof(IRepository<Position, Guid>))]
public class PositionRepository : EfGridRepository<MasterDataDbContext, Position>
{
    public PositionRepository(MasterDataDbContext context, IObjectMapper mapper, ILogger<PositionRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
