using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Outbox;

/// <summary>
/// Фоновый сервис: удаляет обработанные OutboxMessages и старые InboxMessages,
/// чтобы таблицы не росли бесконечно. Запускается раз в OutboxOptions.CleanupInterval.
/// </summary>
public sealed class OutboxCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly OutboxMetrics _metrics;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxCleanupService> _logger;

    public OutboxCleanupService(
        IServiceProvider serviceProvider,
        OutboxMetrics metrics,
        IOptions<OutboxOptions> options,
        ILogger<OutboxCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _metrics = metrics;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox cleanup started. Interval={Interval}, Retention={Retention}, BatchSize={Batch}",
            _options.CleanupInterval, _options.RetentionPeriod, _options.CleanupBatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox cleanup iteration failed");
            }

            try { await Task.Delay(_options.CleanupInterval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        var threshold = DateTimeOffset.UtcNow - _options.RetentionPeriod;

        using var scope = _serviceProvider.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        var inbox = scope.ServiceProvider.GetService<IInboxStore>(); // опционально

        // Удаляем batch'ами, пока есть что удалять.
        var totalOutbox = await DrainAsync(
            () => outbox.DeleteProcessedAsync(threshold, _options.CleanupBatchSize, ct).AsTask(), ct);

        var totalInbox = inbox is null
            ? 0
            : await DrainAsync(
                () => inbox.DeleteOlderThanAsync(threshold, _options.CleanupBatchSize, ct).AsTask(), ct);

        if (totalOutbox > 0)
            _metrics.Cleaned.Add(totalOutbox, new KeyValuePair<string, object?>("table", "outbox"));
        if (totalInbox > 0)
            _metrics.Cleaned.Add(totalInbox, new KeyValuePair<string, object?>("table", "inbox"));

        if (totalOutbox > 0 || totalInbox > 0)
        {
            _logger.LogInformation(
                "Outbox cleanup: removed {OutboxCount} outbox + {InboxCount} inbox rows older than {Threshold}",
                totalOutbox, totalInbox, threshold);
        }
    }

    private static async Task<int> DrainAsync(Func<Task<int>> deleteBatch, CancellationToken ct)
    {
        var total = 0;
        while (!ct.IsCancellationRequested)
        {
            var deleted = await deleteBatch();
            if (deleted == 0)
                break;
            total += deleted;
        }
        return total;
    }
}
