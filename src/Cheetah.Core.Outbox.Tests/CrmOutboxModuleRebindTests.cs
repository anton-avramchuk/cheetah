using System.Reflection;
using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cheetah.Core.Outbox.Tests;

/// <summary>
/// Регресс на баг: <c>CrmOutboxModule.RebindEventBus</c> использовал <c>LastOrDefault(ServiceType == IEventBus)</c>
/// без фильтра по keyed-регистрациям. Модули транспорта (например <c>CrmBackendEventsRedisModule</c>)
/// регистрируют IEventBus дважды — обычной и keyed-регистрацией (для <c>[FromKeyedServices]</c>) — и
/// keyed-дескриптор, добавленный ПОСЛЕ обычного, попадал под <c>LastOrDefault</c> вместо настоящей
/// регистрации транспорта, роняя хост с "неизвестный вид ServiceDescriptor".
/// </summary>
public class CrmOutboxModuleRebindTests
{
    private sealed class FakeEventBus : IEventBus
    {
        public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IEvent
            => ValueTask.CompletedTask;
        public ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken ct = default) where TEvent : IEvent
            => ValueTask.CompletedTask;
        public void Subscribe<TEvent, THandler>() where TEvent : IEvent where THandler : IEventHandler<TEvent> { }
    }

    private static void InvokeRebindEventBus(IServiceCollection services)
    {
        var method = typeof(CrmOutboxModule).GetMethod(
            "RebindEventBus", BindingFlags.NonPublic | BindingFlags.Static);
        method.ShouldNotBeNull();
        method!.Invoke(null, [services]);
    }

    [Fact]
    public void RebindEventBus_ignores_keyed_registration_added_after_real_transport()
    {
        var services = new ServiceCollection();
        var transportInstance = new FakeEventBus();

        // Как CrmBackendEventsRedisModule: сперва обычная регистрация транспорта (через фабрику,
        // как делает генератор [Export]), затем ДОПОЛНИТЕЛЬНО keyed-регистрация того же типа.
        services.AddSingleton<IEventBus>(_ => transportInstance);
        services.AddKeyedSingleton<IEventBus>("redis", (sp, _) => sp.GetRequiredService<IEventBus>());

        Should.NotThrow(() => InvokeRebindEventBus(services));

        using var provider = services.BuildServiceProvider();
        var inner = provider.GetRequiredService<IInnerEventBus>();
        inner.ShouldNotBeNull();
    }

    [Fact]
    public void RebindEventBus_throws_when_no_real_transport_registered()
    {
        var services = new ServiceCollection();
        // Только keyed-регистрация, без обычной — транспорт не подключён.
        services.AddKeyedSingleton<IEventBus>("redis", (sp, _) => new FakeEventBus());

        var ex = Should.Throw<TargetInvocationException>(() => InvokeRebindEventBus(services));
        ex.InnerException.ShouldBeOfType<InvalidOperationException>();
    }
}
