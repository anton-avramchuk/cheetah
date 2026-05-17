using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Audit.Kafka;

/// <summary>
/// Фоновый сервис: читает unpublished AuditEntry из IAuditPublishStore и produces в Kafka.
/// Retry, backoff и SKIP-LOCKED семантика на уровне store (см. PostgresAuditPublishStore).
///
/// Lifecycle: singleton-producer на инстанс приложения (Confluent.Kafka producer thread-safe
/// и сам внутри батчит, держит пул TCP-соединений к брокерам).
/// </summary>
public sealed class KafkaAuditPublisher : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IServiceProvider _serviceProvider;
    private readonly IKafkaProducerFactory _producerFactory;
    private readonly KafkaAuditOptions _options;
    private readonly ILogger<KafkaAuditPublisher> _logger;

    public KafkaAuditPublisher(
        IServiceProvider serviceProvider,
        IKafkaProducerFactory producerFactory,
        IOptions<KafkaAuditOptions> options,
        ILogger<KafkaAuditPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _producerFactory = producerFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var producer = _producerFactory.Create(_options);
        _logger.LogInformation("KafkaAuditPublisher started. Topic={Topic}, BatchSize={Batch}", _options.Topic, _options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(producer, stoppingToken).ConfigureAwait(false);
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

    private async Task ProcessBatchAsync(IProducer<string, string> producer, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAuditPublishStore>();

        var claimTimeout = _options.PollingInterval + TimeSpan.FromSeconds(30);
        var batch = await store.ClaimPendingAsync(_options.BatchSize, claimTimeout, ct).ConfigureAwait(false);
        if (batch.Count == 0) return;

        var successful = new List<Guid>(batch.Count);

        foreach (var entry in batch)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var dto = ToDto(entry);
                var json = JsonSerializer.Serialize(dto, JsonOptions);
                var message = new Message<string, string>
                {
                    Key = $"{entry.EntityType}:{entry.EntityId}",
                    Value = json
                };
                await producer.ProduceAsync(_options.Topic, message, ct).ConfigureAwait(false);
                successful.Add(entry.Id);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogWarning(ex, "Kafka produce failed for AuditEntry {Id}", entry.Id);
                await store.MarkFailedAsync(entry.Id, ex.ToString(), ComputeNextAttempt(entry.RetryCount), ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected failure publishing AuditEntry {Id}", entry.Id);
                await store.MarkFailedAsync(entry.Id, ex.ToString(), ComputeNextAttempt(entry.RetryCount), ct).ConfigureAwait(false);
            }
        }

        if (successful.Count > 0)
        {
            await store.MarkPublishedAsync(successful, ct).ConfigureAwait(false);
            _logger.LogDebug("Published {Count} audit entries to Kafka", successful.Count);
        }
    }

    private DateTimeOffset ComputeNextAttempt(int retryCount)
    {
        var seconds = _options.BaseRetryDelay.TotalSeconds * Math.Pow(2, retryCount);
        var delay = TimeSpan.FromSeconds(Math.Min(seconds, _options.MaxRetryDelay.TotalSeconds));
        return DateTimeOffset.UtcNow + delay;
    }

    private static AuditEntryDto ToDto(AuditEntry e) => new(
        id: e.Id,
        entityType: e.EntityType,
        entityId: e.EntityId,
        action: e.Action.ToString(),
        changes: e.Changes,
        occurredAt: e.OccurredAt,
        userId: e.UserId,
        userName: e.UserName,
        tenantId: e.TenantId,
        correlationId: e.CorrelationId);
}
