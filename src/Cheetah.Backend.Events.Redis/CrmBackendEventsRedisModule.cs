using Cheetah.Backend.Redis;
using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Backend.Events.Redis;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBackendRedisModule), typeof(CrmEventsCoreModule))]
public partial class CrmBackendEventsRedisModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<RedisEventBusOptions>(configuration.GetSection("RedisEventBus"));

        RegisterServices(context.Services);

        // Дополнительная keyed-регистрация "redis": тот же singleton-инстанс доступен и через
        // [FromKeyedServices(EventBusKeys.Redis)] IEventBus, и через дефолтный IEventBus (его
        // создаёт RegisterServices через [Export]).
        context.Services.AddKeyedSingleton<IEventBus>(EventBusKeys.Redis,
            (sp, _) => sp.GetRequiredService<IEventBus>());
    }
}