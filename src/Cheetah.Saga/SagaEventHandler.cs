using Cheetah.Core.Events;

namespace Cheetah.Saga;

/// <summary>
/// Generic-обёртка, регистрируемая в DI как IEventHandler&lt;TEvent&gt;.
/// При получении события просто делегирует ISagaOrchestrator, который сам разрулит
/// какие саги триггерить.
/// </summary>
public sealed class SagaEventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
{
    private readonly ISagaOrchestrator _orchestrator;

    public SagaEventHandler(ISagaOrchestrator orchestrator) => _orchestrator = orchestrator;

    public ValueTask HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
        => _orchestrator.HandleAsync(@event, cancellationToken);
}
