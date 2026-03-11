using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.MasterData.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.MasterData.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Location>), typeof(IRepository<Location, Guid>))]
public class LocationRepository : EfGridRepository<MasterDataDbContext, Location>
{
    public LocationRepository(MasterDataDbContext context, IObjectMapper mapper, ILogger<LocationRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
