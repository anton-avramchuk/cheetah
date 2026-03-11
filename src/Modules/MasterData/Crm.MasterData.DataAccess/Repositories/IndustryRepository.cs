using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.MasterData.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.MasterData.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Industry>), typeof(IRepository<Industry, Guid>))]
public class IndustryRepository : EfGridRepository<MasterDataDbContext, Industry>
{
    public IndustryRepository(MasterDataDbContext context, IObjectMapper mapper, ILogger<IndustryRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
