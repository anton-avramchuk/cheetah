using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Audit.Kafka;

/// <summary>
/// Фоновый сервис: читает unpublished AuditEntry через IAuditPublishStore и публикует
/// через keyed IEventBus(EventBusKeys.Kafka) — то есть через Cheetah.Backend.Events.Kafka.
///
/// Сам не работает с Kafka напрямую — это полностью делегировано CrmKafkaEventBus.
/// </summary>
public sealed class KafkaAuditPublisher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaAuditOptions _options;
    private readonly ILogger<KafkaAuditPublisher> _logger;

    public KafkaAuditPublisher(
        IServiceProvider serviceProvider,
        IOptions<KafkaAuditOptions> options,
        ILogger<KafkaAuditPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KafkaAuditPublisher started. BatchSize={Batch}, Interval={Interval}",
            _options.BatchSize, _options.PollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KafkaAuditPublisher batch failed");
            }

            try { await Task.Delay(_options.PollingInterval, stoppingToken).ConfigureAwait(false); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAuditPublishStore>();
        var bus = scope.ServiceProvider.GetRequiredKeyedService<IEventBus>(EventBusKeys.Kafka);

        var claimTimeout = _options.PollingInterval + TimeSpan.FromSeconds(30);
        var batch = await store.ClaimPendingAsync(_options.BatchSize, claimTimeout, ct).ConfigureAwait(false);
        if (batch.Count == 0) return;

        var successful = new List<Guid>(batch.Count);

        foreach (var entry in batch)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var @event = ToEvent(entry);
                await bus.PublishAsync(@event, ct).ConfigureAwait(false);
                successful.Add(entry.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish AuditEntry {Id}", entry.Id);
                await store.MarkFailedAsync(entry.Id, ex.ToString(),
                    ComputeNextAttempt(entry.RetryCount), ct).ConfigureAwait(false);
            }
        }

        if (successful.Count > 0)
        {
            await store.MarkPublishedAsync(successful, ct).ConfigureAwait(false);
            _logger.LogDebug("Published {Count} audit entries", successful.Count);
        }
    }

    private DateTimeOffset ComputeNextAttempt(int retryCount)
    {
        var seconds = _options.BaseRetryDelay.TotalSeconds * Math.Pow(2, retryCount);
        var delay = TimeSpan.FromSeconds(Math.Min(seconds, _options.MaxRetryDelay.TotalSeconds));
        return DateTimeOffset.UtcNow + delay;
    }

    private static AuditEntryRecordedEvent ToEvent(AuditEntry e) => new()
    {
        EventId = e.Id,
        AuditEntryId = e.Id,
        EntityType = e.EntityType,
        EntityId = e.EntityId,
        Action = e.Action.ToString(),
        Changes = e.Changes,
        OccurredAt = e.OccurredAt,
        UserId = e.UserId,
        UserName = e.UserName,
        TenantId = e.TenantId,
        CorrelationId = e.CorrelationId
    };
}
