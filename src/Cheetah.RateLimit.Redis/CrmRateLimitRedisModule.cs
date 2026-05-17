using Cheetah.Backend.Redis;
using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.RateLimit.Redis;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBackendRedisModule))]
public partial class CrmRateLimitRedisModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Configure<RedisRateLimitOptions>(services.GetConfiguration().GetSection("RateLimit"));
        RegisterServices(services);
    }
}
