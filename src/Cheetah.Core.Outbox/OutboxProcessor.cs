using System.Reflection;
using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Outbox;

/// <summary>
/// Фоновый сервис, который вычитывает необработанные OutboxMessage и публикует их через IInnerEventBus.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private static readonly MethodInfo PublishAsyncMethod = typeof(IEventBus)
        .GetMethod(nameof(IEventBus.PublishAsync))
        ?? throw new InvalidOperationException("IEventBus.PublishAsync not found");

    private static readonly MethodInfo PublishManyAsyncMethod = typeof(IEventBus)
        .GetMethod(nameof(IEventBus.PublishManyAsync))
        ?? throw new InvalidOperationException("IEventBus.PublishManyAsync not found");

    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxNotifier _notifier;
    private readonly OutboxMetrics _metrics;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        IOutboxNotifier notifier,
        OutboxMetrics metrics,
        IOptions<OutboxOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _notifier = notifier;
        _metrics = metrics;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started. Polling every {Interval}, batch={Batch}",
            _options.PollingInterval, _options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processor batch failed");
            }

            // Просыпаемся либо по polling-таймеру (fallback + retry), либо по внешнему сигналу
            // (LISTEN/NOTIFY в реализации Postgres). Что наступит раньше.
            using var combined = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var delay = Task.Delay(_options.PollingInterval, combined.Token);
            var signal = _notifier.WaitForSignalAsync(combined.Token).AsTask();

            await Task.WhenAny(delay, signal);
            combined.Cancel(); // отменяем второго ожидающего, чтобы не висел

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        var inner = scope.ServiceProvider.GetRequiredService<IInnerEventBus>();
        var dlq = scope.ServiceProvider.GetService<IDeadLetterStore>(); // опционально

        var pending = await store.GetPendingAsync(_options.BatchSize, ct);
        if (pending.Count == 0)
        {
            return;
        }

        // Группируем по EventType: PublishManyAsync<TEvent> экономит round-trip'ы в транспорт.
        foreach (var group in pending.GroupBy(m => m.EventType))
        {
            ct.ThrowIfCancellationRequested();
            await ProcessGroupAsync(store, inner, dlq, group.Key, group.ToList(), ct);
        }
    }

    private async Task ProcessGroupAsync(
        IOutboxStore store, IInnerEventBus inner, IDeadLetterStore? dlq, string eventTypeName, IReadOnlyList<OutboxMessage> messages, CancellationToken ct)
    {
        var startedAt = System.Diagnostics.Stopwatch.GetTimestamp();
        Type? eventType = null;
        Array? eventsArray = null;
        try
        {
            // Десериализуем всю группу в типизированный массив TEvent[].
            for (var i = 0; i < messages.Count; i++)
            {
                var (type, @event) = OutboxEventSerializer.Deserialize(messages[i]);
                eventType ??= type;
                if (type != eventType)
                {
                    // Один и тот же EventType.AssemblyQualifiedName, но разные Type? Аномалия.
                    throw new InvalidOperationException(
                        $"Inconsistent event type within group '{eventTypeName}'");
                }
                eventsArray ??= Array.CreateInstance(eventType, messages.Count);
                eventsArray.SetValue(@event, i);
            }

            if (messages.Count == 1)
            {
                var generic = PublishAsyncMethod.MakeGenericMethod(eventType!);
                var task = (ValueTask)generic.Invoke(inner, new object[] { eventsArray!.GetValue(0)!, ct })!;
                await task;
            }
            else
            {
                var generic = PublishManyAsyncMethod.MakeGenericMethod(eventType!);
                var task = (ValueTask)generic.Invoke(inner, new object[] { eventsArray!, ct })!;
                await task;
            }

            // Все ушли успехом — батчем помечаем processed.
            foreach (var msg in messages)
            {
                await store.MarkProcessedAsync(msg.Id, ct);
            }

            var elapsedMs = System.Diagnostics.Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            _metrics.PublishLatencyMs.Record(elapsedMs / messages.Count,
                new KeyValuePair<string, object?>("event_type", eventTypeName));
            _metrics.Published.Add(messages.Count,
                new KeyValuePair<string, object?>("event_type", eventTypeName));
        }
        catch (Exception batchEx) when (messages.Count > 1)
        {
            // Батч упал целиком — мы не знаем, какое сообщение виновато.
            // Откатываемся на per-message retry, чтобы точно посчитать RetryCount по каждому.
            _logger.LogWarning(batchEx, "Batch publish failed for '{EventType}' ({Count} messages); falling back to per-message",
                eventTypeName, messages.Count);

            foreach (var msg in messages)
            {
                await ProcessGroupAsync(store, inner, dlq, eventTypeName, new[] { msg }, ct);
            }
        }
        catch (Exception ex)
        {
            // messages.Count == 1: одиночное сообщение упало.
            var msg = messages[0];
            var nextRetry = msg.RetryCount + 1;

            // Превышен лимит → DLQ (если зарегистрирован), иначе откладываем "далеко в будущее".
            if (nextRetry > _options.MaxRetries && dlq is not null)
            {
                _logger.LogError(ex, "Outbox message {MessageId} exceeded MaxRetries ({Max}); moving to dead-letter",
                    msg.Id, _options.MaxRetries);

                await dlq.MoveFromOutboxAsync(msg, ex.ToString(), ct);
                _metrics.Failed.Add(1,
                    new KeyValuePair<string, object?>("event_type", eventTypeName),
                    new KeyValuePair<string, object?>("dead_letter", true));
                return;
            }

            var nextAttempt = ComputeNextAttempt(msg.RetryCount);
            _logger.LogWarning(ex, "Failed to publish outbox message {MessageId} (attempt {Retry}); next attempt at {NextAttempt}",
                msg.Id, nextRetry, nextAttempt);

            await store.MarkFailedAsync(msg.Id, ex.ToString(), nextAttempt, ct);
            _metrics.Failed.Add(1, new KeyValuePair<string, object?>("event_type", eventTypeName));
        }
    }

    private DateTimeOffset ComputeNextAttempt(int retryCount)
    {
        var seconds = _options.BaseRetryDelay.TotalSeconds * Math.Pow(2, retryCount);
        var delay = TimeSpan.FromSeconds(Math.Min(seconds, _options.MaxRetryDelay.TotalSeconds));
        return DateTimeOffset.UtcNow + delay;
    }
}
