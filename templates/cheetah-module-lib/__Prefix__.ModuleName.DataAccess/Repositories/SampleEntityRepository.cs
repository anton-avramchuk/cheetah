using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using __Prefix__.ModuleName.Domain;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.Logging;

namespace __Prefix__.ModuleName.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<SampleEntity>), typeof(IRepository<SampleEntity, Guid>))]
public class SampleEntityRepository : EfGridRepository<ModuleNameDbContext, SampleEntity>
{
    public SampleEntityRepository(ModuleNameDbContext context, IObjectMapper mapper, ILogger<SampleEntityRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
