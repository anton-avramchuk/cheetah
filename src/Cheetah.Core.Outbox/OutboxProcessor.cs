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

    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxNotifier _notifier;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        IOutboxNotifier notifier,
        IOptions<OutboxOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _notifier = notifier;
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

        var pending = await store.GetPendingAsync(_options.BatchSize, ct);
        if (pending.Count == 0)
        {
            return;
        }

        foreach (var message in pending)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var (type, @event) = OutboxEventSerializer.Deserialize(message);
                var generic = PublishAsyncMethod.MakeGenericMethod(type);
                var task = (ValueTask)generic.Invoke(inner, new object[] { @event, ct })!;
                await task;

                await store.MarkProcessedAsync(message.Id, ct);
            }
            catch (Exception ex)
            {
                var nextAttempt = ComputeNextAttempt(message.RetryCount);
                _logger.LogWarning(ex, "Failed to publish outbox message {MessageId} (attempt {Retry}); next attempt at {NextAttempt}",
                    message.Id, message.RetryCount + 1, nextAttempt);

                await store.MarkFailedAsync(message.Id, ex.ToString(), nextAttempt, ct);
            }
        }
    }

    private DateTimeOffset ComputeNextAttempt(int retryCount)
    {
        var seconds = _options.BaseRetryDelay.TotalSeconds * Math.Pow(2, retryCount);
        var delay = TimeSpan.FromSeconds(Math.Min(seconds, _options.MaxRetryDelay.TotalSeconds));
        return DateTimeOffset.UtcNow + delay;
    }
}
