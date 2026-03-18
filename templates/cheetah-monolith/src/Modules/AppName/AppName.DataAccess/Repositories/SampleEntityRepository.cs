using AppName.Domain;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.Logging;

namespace AppName.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<SampleEntity>), typeof(IRepository<SampleEntity, Guid>))]
public class SampleEntityRepository : EfGridRepository<AppNameDbContext, SampleEntity>
{
    public SampleEntityRepository(AppNameDbContext context, IObjectMapper mapper, ILogger<SampleEntityRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
