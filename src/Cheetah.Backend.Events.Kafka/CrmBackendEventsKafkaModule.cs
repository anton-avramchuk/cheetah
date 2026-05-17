using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Backend.Events.Kafka;

/// <summary>
/// Регистрирует CrmKafkaEventBus как keyed IEventBus(EventBusKeys.Kafka).
/// НЕ перекрывает дефолтную регистрацию IEventBus — она остаётся за тем транспортом,
/// который зарегистрировал её обычным способом (обычно Cheetah.Backend.Events.Redis).
///
/// Использование:
/// <code>
/// public class AuditPublisher([FromKeyedServices(EventBusKeys.Kafka)] IEventBus bus) { }
/// </code>
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class CrmBackendEventsKafkaModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Configure<KafkaEventBusOptions>(services.GetConfiguration().GetSection("KafkaEventBus"));
        services.TryAddSingleton<IKafkaProducerFactory, DefaultKafkaProducerFactory>();

        // Singleton CrmKafkaEventBus + два способа его достать:
        // 1) typed concrete (для Dispose lifecycle)
        // 2) keyed IEventBus("kafka")
        services.AddSingleton<CrmKafkaEventBus>();
        services.AddKeyedSingleton<IEventBus>(EventBusKeys.Kafka,
            (sp, _) => sp.GetRequiredService<CrmKafkaEventBus>());
    }
}
