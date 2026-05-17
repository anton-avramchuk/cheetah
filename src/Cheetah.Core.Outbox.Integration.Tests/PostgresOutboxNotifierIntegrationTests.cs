using System.Diagnostics;
using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using Shouldly;

namespace Cheetah.Core.Outbox.Integration.Tests;

[Collection("Postgres")]
public class PostgresOutboxNotifierIntegrationTests
{
    private readonly PostgresFixture _fx;

    public PostgresOutboxNotifierIntegrationTests(PostgresFixture fx) => _fx = fx;

    [Fact]
    public async Task LISTEN_NOTIFY_сигналит_быстрее_polling_intervala()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new PostgresOutboxOptions
        {
            ChannelName = "outbox_new",
            ConnectionString = _fx.ConnectionString,
            BaseReconnectDelay = TimeSpan.FromMilliseconds(100)
        });

        var configuration = new ConfigurationBuilder().Build();
        var notifier = new PostgresOutboxNotifier(options, configuration, NullLogger<PostgresOutboxNotifier>.Instance);

        using var stop = new CancellationTokenSource();
        var serviceTask = notifier.StartAsync(stop.Token);
        await serviceTask;

        // Даём notifier'у время подключиться и сделать LISTEN.
        // Первый Pulse приходит сразу при подключении — съедаем его.
        await Task.Delay(500);
        await WaitWithTimeout(notifier, TimeSpan.FromMilliseconds(200));

        // Теперь запускаем долгое ожидание и параллельно эмулируем NOTIFY через прямой INSERT в таблицу.
        var sw = Stopwatch.StartNew();
        var waitTask = notifier.WaitForSignalAsync(stop.Token).AsTask();

        // Небольшая задержка чтобы wait встал в очередь
        await Task.Delay(100);

        await using var conn = new NpgsqlConnection(_fx.ConnectionString);
        await conn.OpenAsync();
        await using (var cmd = new NpgsqlCommand("SELECT pg_notify('outbox_new', 'test-payload');", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await waitTask.WaitAsync(TimeSpan.FromSeconds(5));
        sw.Stop();

        sw.ElapsedMilliseconds.ShouldBeLessThan(2000, "LISTEN должен срабатывать мгновенно после NOTIFY");

        await notifier.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task INSERT_в_OutboxMessages_триггерит_NOTIFY()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new PostgresOutboxOptions
        {
            ChannelName = "outbox_new",
            ConnectionString = _fx.ConnectionString
        });
        var notifier = new PostgresOutboxNotifier(options, new ConfigurationBuilder().Build(),
            NullLogger<PostgresOutboxNotifier>.Instance);

        using var stop = new CancellationTokenSource();
        await notifier.StartAsync(stop.Token);

        await Task.Delay(500);
        await WaitWithTimeout(notifier, TimeSpan.FromMilliseconds(200)); // съесть initial pulse

        var waitTask = notifier.WaitForSignalAsync(stop.Token).AsTask();
        await Task.Delay(100);

        // INSERT через DbContext → срабатывает наш SQL-триггер → pg_notify('outbox_new', ...)
        await using (var db = _fx.CreateDbContext())
        {
            db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "TriggerTest, Asm",
                Payload = "{}",
                OccurredAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        await waitTask.WaitAsync(TimeSpan.FromSeconds(5));

        await notifier.StopAsync(CancellationToken.None);
    }

    private static async Task WaitWithTimeout(PostgresOutboxNotifier notifier, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try { await notifier.WaitForSignalAsync(cts.Token); }
        catch (OperationCanceledException) { /* timeout — ок, означает что pending pulse не было */ }
    }
}
