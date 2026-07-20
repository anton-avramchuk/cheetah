using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Cheetah.Core.Outbox.PostgreSql.Tests;

/// <summary>
/// Ожидание сигнала нотификатором.
/// <para>
/// Здесь без Postgres: проверяется поведение самого ожидания, а не LISTEN/NOTIFY. Именно оно
/// определяет, спит ли <c>OutboxProcessor</c> между опросами. Processor каждую итерацию ждёт
/// «сигнал ИЛИ таймер» на связанном токене и затем токен отменяет, поэтому отмена ожидания —
/// не исключительная ситуация, а норма, повторяющаяся в цикле.
/// </para>
/// </summary>
public class PostgresOutboxNotifierTests
{
    private static PostgresOutboxNotifier CreateNotifier()
        => new(
            Microsoft.Extensions.Options.Options.Create(new PostgresOutboxOptions { ConnectionString = "Host=localhost;Database=unused" }),
            new ConfigurationBuilder().Build(),
            NullLogger<PostgresOutboxNotifier>.Instance);

    [Fact]
    public async Task Wait_returns_when_its_own_token_is_cancelled()
    {
        var notifier = CreateNotifier();
        using var cts = new CancellationTokenSource();

        var waiting = notifier.WaitForSignalAsync(cts.Token).AsTask();
        await cts.CancelAsync();

        await waiting.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Cancelled_wait_does_not_burn_the_signal_for_the_next_one()
    {
        // Регрессия: отмена одного ожидания завершала ОБЩИЙ сигнал, и все последующие
        // ожидания возвращались мгновенно. Processor переставал спать и опрашивал базу
        // в тесном цикле — в логах это ~30 SQL-запросов в секунду на каждый сервис
        // (расследование: логи Aspire на 70 млн строк и гигабайты на прогон).
        var notifier = CreateNotifier();

        using (var first = new CancellationTokenSource())
        {
            var abandoned = notifier.WaitForSignalAsync(first.Token).AsTask();
            await first.CancelAsync();
            await abandoned.WaitAsync(TimeSpan.FromSeconds(1));
        }

        var next = notifier.WaitForSignalAsync(CancellationToken.None).AsTask();

        // Сигнала не было — ожидание обязано висеть.
        var finished = await Task.WhenAny(next, Task.Delay(TimeSpan.FromMilliseconds(300)));
        finished.ShouldNotBe(next, "ожидание сигнала завершилось само, без уведомления из Postgres");
    }

    [Fact]
    public async Task Wait_returns_when_a_notification_arrives()
    {
        var notifier = CreateNotifier();

        var waiting = notifier.WaitForSignalAsync(CancellationToken.None).AsTask();
        notifier.Pulse();

        await waiting.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Waiter_that_survived_a_neighbours_cancellation_still_wakes_on_notification()
    {
        // Два ожидающих на одном сигнале: отмена одного не должна ни разбудить, ни «оглушить» второго.
        var notifier = CreateNotifier();
        using var cts = new CancellationTokenSource();

        var cancelled = notifier.WaitForSignalAsync(cts.Token).AsTask();
        var alive = notifier.WaitForSignalAsync(CancellationToken.None).AsTask();

        await cts.CancelAsync();
        await cancelled.WaitAsync(TimeSpan.FromSeconds(1));
        alive.IsCompleted.ShouldBeFalse("отмена соседа разбудила чужое ожидание");

        notifier.Pulse();
        await alive.WaitAsync(TimeSpan.FromSeconds(1));
    }
}
