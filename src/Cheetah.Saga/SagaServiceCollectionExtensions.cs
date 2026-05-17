using System.Reflection;
using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Saga;

/// <summary>
/// Внутренний маркер для накопления типов саг в DI до момента создания SagaRegistry.
/// </summary>
internal sealed record SagaTypeMarker(Type Type);

public static class SagaServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует тип саги:
    /// 1) сам класс TSaga как Transient (orchestrator создаёт новый instance под каждое событие);
    /// 2) маркер SagaTypeMarker — потом SagaRegistry соберёт их все в свой index;
    /// 3) SagaEventHandler&lt;T&gt; как IEventHandler&lt;T&gt; для каждого события из [SagaStartedBy]/[SagaHandles].
    /// </summary>
    public static IServiceCollection AddSaga<TSaga>(this IServiceCollection services)
        where TSaga : class, ISaga
    {
        var sagaType = typeof(TSaga);

        services.AddTransient<TSaga>();
        services.AddSingleton(new SagaTypeMarker(sagaType));

        // SagaRegistry собирается один раз из всех зарегистрированных маркеров.
        services.TryAddSingleton<SagaRegistry>(sp =>
        {
            var registry = new SagaRegistry();
            foreach (var marker in sp.GetServices<SagaTypeMarker>())
                registry.Register(marker.Type);
            return registry;
        });

        foreach (var attr in sagaType.GetCustomAttributes<SagaStartedByAttribute>())
            RegisterEventHandler(services, attr.EventType);
        foreach (var attr in sagaType.GetCustomAttributes<SagaHandlesAttribute>())
            RegisterEventHandler(services, attr.EventType);

        return services;
    }

    private static void RegisterEventHandler(IServiceCollection services, Type eventType)
    {
        if (!typeof(IEvent).IsAssignableFrom(eventType))
            throw new ArgumentException($"{eventType.FullName} must implement IEvent", nameof(eventType));

        var handlerInterface = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlerImpl = typeof(SagaEventHandler<>).MakeGenericType(eventType);

        // Несколько саг могут реагировать на один и тот же event — но IEventHandler регистрируется
        // как single. Проверяем, не зарегистрирован ли уже SagaEventHandler<T> и пропускаем.
        if (services.Any(d => d.ServiceType == handlerInterface && d.ImplementationType == handlerImpl))
            return;

        services.AddScoped(handlerInterface, handlerImpl);
    }
}
