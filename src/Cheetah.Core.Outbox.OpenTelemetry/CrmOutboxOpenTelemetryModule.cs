using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Core.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Core.Outbox.OpenTelemetry;

/// <summary>
/// Подключает OpenTelemetry-метрики Outbox.
/// Заменяет NullOutboxMetrics (зарегистрированный CrmOutboxModule'ом) на OpenTelemetryOutboxMetrics.
/// Для экспорта добавьте в OTel-builder: AddMeter(OpenTelemetryOutboxMetrics.MeterName).
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmOutboxModule))]
public partial class CrmOutboxOpenTelemetryModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Replace(ServiceDescriptor.Singleton<IOutboxMetrics, OpenTelemetryOutboxMetrics>());
    }
}
