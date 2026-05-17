using Cheetah.Backend.Events.Kafka;
using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Audit.Kafka;

/// <summary>
/// Сам Kafka-транспорт подключается через CrmBackendEventsKafkaModule — этот модуль только
/// добавляет publisher, который читает AuditEntries и шлёт через keyed IEventBus(EventBusKeys.Kafka).
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CrmAuditModule), typeof(CrmBackendEventsKafkaModule))]
public partial class CrmAuditKafkaModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Configure<KafkaAuditOptions>(services.GetConfiguration().GetSection("Audit:Kafka"));
        services.AddSingleton<IHostedService, KafkaAuditPublisher>();
    }
}
