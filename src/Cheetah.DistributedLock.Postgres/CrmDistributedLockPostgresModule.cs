using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.DistributedLock.Postgres;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDistributedLockModule))]
public partial class CrmDistributedLockPostgresModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Configure<PostgresLockOptions>(services.GetConfiguration().GetSection("DistributedLock:Postgres"));
        RegisterServices(services);
    }
}
