using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Cheetah.Core.Outbox.PostgreSql;

/// <summary>
/// Слушает Postgres LISTEN/NOTIFY на канале и сигналит OutboxProcessor'у через IOutboxNotifier.
/// Запускается как BackgroundService и одновременно сам реализует IOutboxNotifier:
/// каждое уведомление сбрасывает внутренний TaskCompletionSource, который ждёт processor.
///
/// При разрыве коннекта переподключается с экспоненциальным backoff.
/// Если NOTIFY теряется (overflow или окно реконнекта) — polling-таймер processor'а
/// подхватит сообщения как обычно.
/// </summary>
public sealed class PostgresOutboxNotifier : BackgroundService, IOutboxNotifier
{
    private readonly PostgresOutboxOptions _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PostgresOutboxNotifier> _logger;

    // Lazily-created TCS, который ждёт processor. Сбрасывается при каждом уведомлении.
    private TaskCompletionSource _signal = NewSignal();

    public PostgresOutboxNotifier(
        IOptions<PostgresOutboxOptions> options,
        IConfiguration configuration,
        ILogger<PostgresOutboxNotifier> logger)
    {
        _options = options.Value;
        _configuration = configuration;
        _logger = logger;
    }

    public async ValueTask WaitForSignalAsync(CancellationToken cancellationToken)
    {
        var signal = Volatile.Read(ref _signal);
        using var registration = cancellationToken.Register(static s => ((TaskCompletionSource)s!).TrySetResult(), signal);
        await signal.Task;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var attempt = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ListenLoopAsync(stoppingToken);
                attempt = 0;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                attempt++;
                var delay = ComputeReconnectDelay(attempt);
                _logger.LogWarning(ex, "LISTEN connection failed (attempt {Attempt}); reconnecting in {Delay}",
                    attempt, delay);
                try { await Task.Delay(delay, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private async Task ListenLoopAsync(CancellationToken stoppingToken)
    {
        var connString = ResolveConnectionString();
        await using var conn = new NpgsqlConnection(connString);
        conn.Notification += OnNotification;

        await conn.OpenAsync(stoppingToken);
        await using (var cmd = new NpgsqlCommand($"LISTEN {QuoteIdentifier(_options.ChannelName)};", conn))
        {
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }

        _logger.LogInformation("PostgresOutboxNotifier: LISTEN on channel '{Channel}'", _options.ChannelName);

        // Будим сразу: возможно, в outbox уже что-то есть с прошлой жизни процесса.
        Pulse();

        while (!stoppingToken.IsCancellationRequested)
        {
            await conn.WaitAsync(stoppingToken);
        }
    }

    private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
    {
        // На каждое уведомление дёргаем сигнал.
        Pulse();
    }

    private void Pulse()
    {
        var fresh = NewSignal();
        var old = Interlocked.Exchange(ref _signal, fresh);
        old.TrySetResult();
    }

    private TimeSpan ComputeReconnectDelay(int attempt)
    {
        var seconds = _options.BaseReconnectDelay.TotalSeconds * Math.Pow(2, attempt - 1);
        return TimeSpan.FromSeconds(Math.Min(seconds, _options.MaxReconnectDelay.TotalSeconds));
    }

    private string ResolveConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            return _options.ConnectionString;
        }

        var fromConfig = _configuration.GetConnectionString(_options.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(fromConfig))
        {
            throw new InvalidOperationException(
                $"PostgresOutboxNotifier: не задана ConnectionString и не найдена секция " +
                $"ConnectionStrings:{_options.ConnectionStringName}.");
        }
        return fromConfig;
    }

    /// <summary>
    /// Минимальный безопасный экранировщик для имени канала pg_notify.
    /// Допускаем только [A-Za-z0-9_], чтобы исключить SQL-инъекцию через имя канала.
    /// </summary>
    private static string QuoteIdentifier(string name)
    {
        foreach (var c in name)
        {
            if (!(char.IsLetterOrDigit(c) || c == '_'))
            {
                throw new ArgumentException($"Invalid channel name: '{name}'. Only [A-Za-z0-9_] allowed.");
            }
        }
        return name;
    }

    private static TaskCompletionSource NewSignal()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
