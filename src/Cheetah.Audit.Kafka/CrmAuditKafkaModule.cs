using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Audit.Kafka;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAuditModule))]
public partial class CrmAuditKafkaModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Configure<KafkaAuditOptions>(services.GetConfiguration().GetSection("Audit:Kafka"));
        services.TryAddSingleton<IKafkaProducerFactory, DefaultKafkaProducerFactory>();
        services.AddSingleton<IHostedService, KafkaAuditPublisher>();
    }
}
