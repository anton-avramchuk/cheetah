using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.MasterData.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.MasterData.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<WorkFormat>), typeof(IRepository<WorkFormat, Guid>))]
public class WorkFormatRepository : EfGridRepository<MasterDataDbContext, WorkFormat>
{
    public WorkFormatRepository(MasterDataDbContext context, IObjectMapper mapper, ILogger<WorkFormatRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
