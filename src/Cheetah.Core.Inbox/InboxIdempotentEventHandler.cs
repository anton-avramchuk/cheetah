using Cheetah.Core.Events;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Inbox;

/// <summary>
/// Декоратор IEventHandler, обеспечивающий идемпотентную обработку события:
/// перед вызовом inner-хендлера проверяется (EventId, ConsumerName) в IInboxStore,
/// после успешной обработки запись добавляется в Inbox.
///
/// ВАЖНО: inner.HandleAsync должен использовать тот же DbContext, что и IInboxStore,
/// и сам вызывать SaveChangesAsync — иначе атомарность теряется.
/// </summary>
public sealed class InboxIdempotentEventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
{
    private readonly IEventHandler<TEvent> _inner;
    private readonly IInboxStore _inbox;
    private readonly ILogger<InboxIdempotentEventHandler<TEvent>> _logger;
    private readonly string _consumerName;

    public InboxIdempotentEventHandler(
        IEventHandler<TEvent> inner,
        IInboxStore inbox,
        ILogger<InboxIdempotentEventHandler<TEvent>> logger)
    {
        _inner = inner;
        _inbox = inbox;
        _logger = logger;
        _consumerName = inner.GetType().FullName
                        ?? throw new InvalidOperationException("Handler type has no FullName");
    }

    public async ValueTask HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        if (await _inbox.AlreadyProcessedAsync(@event.EventId, _consumerName, cancellationToken))
        {
            _logger.LogDebug("Skipping already-processed event {EventId} for consumer {Consumer}",
                @event.EventId, _consumerName);
            return;
        }

        await _inner.HandleAsync(@event, cancellationToken);

        await _inbox.AddAsync(new InboxMessage
        {
            EventId = @event.EventId,
            ConsumerName = _consumerName,
            EventType = typeof(TEvent).AssemblyQualifiedName!,
            ReceivedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
    }
}
