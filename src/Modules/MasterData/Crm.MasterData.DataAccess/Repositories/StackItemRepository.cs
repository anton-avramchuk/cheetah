using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.MasterData.Domain;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.Logging;

namespace Crm.MasterData.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<StackItem>), typeof(IRepository<StackItem, Guid>))]
public class StackItemRepository : EfGridRepository<MasterDataDbContext, StackItem>
{
    public StackItemRepository(MasterDataDbContext context, IObjectMapper mapper, ILogger<StackItemRepository> logger)
        : base(context, mapper, logger)
    {
    }
}