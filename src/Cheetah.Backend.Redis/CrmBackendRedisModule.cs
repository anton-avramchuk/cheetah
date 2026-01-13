using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Backend.Redis;

[DependsOn(typeof(CoreModule))]
public partial class CrmBackendRedisModule: CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<RedisOptions>(configuration.GetSection("Redis"));

        RegisterServices(context.Services);
    }
}