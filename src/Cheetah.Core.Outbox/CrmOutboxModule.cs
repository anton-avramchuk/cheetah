using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Core.Outbox;

/// <summary>
/// Подключает Outbox-паттерн:
/// 1) переносит существующую регистрацию IEventBus на IInnerEventBus;
/// 2) регистрирует OutboxEventBus как IEventBus (Scoped);
/// 3) запускает OutboxProcessor в фоне.
///
/// ВАЖНО: указывайте этот модуль в [DependsOn] ПОСЛЕ модуля транспорта (Redis/InMemory),
/// чтобы захватить уже зарегистрированный IEventBus.
/// Реализация IOutboxStore подключается отдельным модулем (например, EntityFrameworkCore).
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class CrmOutboxModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));

        RebindEventBus(services);

        RegisterServices(services);

        services.TryAddSingleton<IOutboxNotifier, NullOutboxNotifier>();
        services.AddSingleton<IHostedService, OutboxProcessor>();
    }

    /// <summary>
    /// Снимает существующую регистрацию IEventBus и переносит её на IInnerEventBus.
    /// OutboxEventBus затем регистрируется через [Export] как IEventBus.
    /// </summary>
    private static void RebindEventBus(IServiceCollection services)
    {
        var existing = services.LastOrDefault(s => s.ServiceType == typeof(IEventBus));
        if (existing is null)
        {
            throw new InvalidOperationException(
                "CrmOutboxModule: IEventBus is not registered. " +
                "Подключите транспорт (Cheetah.Backend.Events.Redis / InMemory) до этого модуля.");
        }

        services.Remove(existing);

        // Восстанавливаем оригинальный экземпляр под закрытым типом, чтобы адаптер мог его получить.
        // Используем сам implType как ключ, либо сохраняем factory/instance.
        if (existing.ImplementationType is { } implType)
        {
            services.Add(new ServiceDescriptor(implType, implType, existing.Lifetime));
            services.Add(new ServiceDescriptor(
                typeof(IInnerEventBus),
                sp => new InnerEventBusAdapter((IEventBus)sp.GetRequiredService(implType)),
                existing.Lifetime));
        }
        else if (existing.ImplementationFactory is { } factory)
        {
            services.Add(new ServiceDescriptor(
                typeof(IInnerEventBus),
                sp => new InnerEventBusAdapter((IEventBus)factory(sp)),
                existing.Lifetime));
        }
        else if (existing.ImplementationInstance is IEventBus instance)
        {
            services.Add(new ServiceDescriptor(typeof(IInnerEventBus), new InnerEventBusAdapter(instance)));
        }
        else
        {
            throw new InvalidOperationException(
                "CrmOutboxModule: не удалось перенести регистрацию IEventBus — неизвестный вид ServiceDescriptor.");
        }
    }
}
