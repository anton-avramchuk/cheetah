using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Workflow.Infrastructure.Firehose;

public static class WorkflowFirehoseExtensions
{
    /// <summary>
    /// Декорирует зарегистрированный <see cref="IEventBus"/> форвардером (<see cref="WorkflowEventForwarder"/>),
    /// который дублирует все события в firehose-канал. Вызывается на уровне ХОСТА после регистрации шины
    /// (Redis/Kafka/InMemory/Outbox) — это явное host-решение, поэтому не входит в авто-регистрацию модуля.
    /// </summary>
    public static IServiceCollection AddWorkflowFirehose(this IServiceCollection services)
    {
        var descriptor = services.LastOrDefault(d => d.ServiceType == typeof(IEventBus))
            ?? throw new InvalidOperationException(
                "IEventBus is not registered. Register the event bus (Redis/Kafka/InMemory/Outbox) before AddWorkflowFirehose().");

        services.Remove(descriptor);
        services.Add(new ServiceDescriptor(
            typeof(IEventBus),
            sp => new WorkflowEventForwarder(ResolveInner(sp, descriptor)),
            descriptor.Lifetime));

        return services;
    }

    private static IEventBus ResolveInner(IServiceProvider sp, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance is IEventBus instance)
            return instance;
        if (descriptor.ImplementationFactory is { } factory)
            return (IEventBus)factory(sp);
        if (descriptor.ImplementationType is { } type)
            return (IEventBus)ActivatorUtilities.CreateInstance(sp, type);
        throw new InvalidOperationException("Cannot resolve the inner IEventBus implementation for firehose decoration.");
    }
}
