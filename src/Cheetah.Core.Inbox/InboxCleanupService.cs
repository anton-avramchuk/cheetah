using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Inbox;

/// <summary>
/// Фоновый сервис: удаляет старые InboxMessages, чтобы таблица не росла бесконечно.
/// Запускается раз в InboxOptions.CleanupInterval.
/// </summary>
public sealed class InboxCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInboxMetrics _metrics;
    private readonly InboxOptions _options;
    private readonly ILogger<InboxCleanupService> _logger;

    public InboxCleanupService(
        IServiceProvider serviceProvider,
        IInboxMetrics metrics,
        IOptions<InboxOptions> options,
        ILogger<InboxCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _metrics = metrics;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Inbox cleanup started. Interval={Interval}, Retention={Retention}, BatchSize={Batch}",
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
                _logger.LogError(ex, "Inbox cleanup iteration failed");
            }

            try { await Task.Delay(_options.CleanupInterval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        var threshold = DateTimeOffset.UtcNow - _options.RetentionPeriod;

        using var scope = _serviceProvider.CreateScope();
        var inbox = scope.ServiceProvider.GetService<IInboxStore>();
        if (inbox is null)
            return;

        var total = 0;
        while (!ct.IsCancellationRequested)
        {
            var deleted = await inbox.DeleteOlderThanAsync(threshold, _options.CleanupBatchSize, ct);
            if (deleted == 0)
                break;
            total += deleted;
        }

        if (total > 0)
        {
            _metrics.RecordCleaned(total);
            _logger.LogInformation(
                "Inbox cleanup: removed {Count} rows older than {Threshold}", total, threshold);
        }
    }
}
