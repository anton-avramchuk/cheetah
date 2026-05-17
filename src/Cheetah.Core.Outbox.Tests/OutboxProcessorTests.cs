using Cheetah.Core.Events;
using Cheetah.Core.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Cheetah.Core.Outbox.Tests;

public class OutboxProcessorTests
{
    [Fact]
    public async Task Обрабатывает_pending_сообщения_и_помечает_processed()
    {
        var msg = OutboxEventSerializer.Serialize(new TestEvent("hello"));

        var store = new Mock<IOutboxStore>();
        var firstCall = true;
        store.Setup(s => s.GetPendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                if (firstCall) { firstCall = false; return new ValueTask<IReadOnlyList<OutboxMessage>>(new[] { msg }); }
                return new ValueTask<IReadOnlyList<OutboxMessage>>(Array.Empty<OutboxMessage>());
            });
        store.Setup(s => s.MarkProcessedAsync(msg.Id, It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);

        var inner = new Mock<IInnerEventBus>();
        inner.Setup(b => b.PublishAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var sut = BuildProcessor(store.Object, inner.Object);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await sut.StartAsync(cts.Token);
        await Task.Delay(500);
        await sut.StopAsync(CancellationToken.None);

        inner.Verify(b => b.PublishAsync(It.Is<TestEvent>(e => e.Name == "hello"), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        store.Verify(s => s.MarkProcessedAsync(msg.Id, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task При_ошибке_публикации_зовётся_MarkFailed_с_экспоненциальным_backoff()
    {
        var msg = OutboxEventSerializer.Serialize(new TestEvent("err"));
        msg.RetryCount = 2;

        var store = new Mock<IOutboxStore>();
        var calls = 0;
        store.Setup(s => s.GetPendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                calls++;
                return calls == 1
                    ? new ValueTask<IReadOnlyList<OutboxMessage>>(new[] { msg })
                    : new ValueTask<IReadOnlyList<OutboxMessage>>(Array.Empty<OutboxMessage>());
            });
        DateTimeOffset? recordedNext = null;
        store.Setup(s => s.MarkFailedAsync(msg.Id, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, DateTimeOffset, CancellationToken>((_, _, next, _) => recordedNext = next)
            .Returns(ValueTask.CompletedTask);

        var inner = new Mock<IInnerEventBus>();
        inner.Setup(b => b.PublishAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException("boom"));

        var sut = BuildProcessor(store.Object, inner.Object,
            new OutboxOptions { BaseRetryDelay = TimeSpan.FromSeconds(1), MaxRetryDelay = TimeSpan.FromMinutes(1) });
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await sut.StartAsync(cts.Token);
        await Task.Delay(500);
        await sut.StopAsync(CancellationToken.None);

        recordedNext.ShouldNotBeNull();
        // base=1s * 2^2 = 4s
        var diff = recordedNext!.Value - DateTimeOffset.UtcNow;
        diff.TotalSeconds.ShouldBeInRange(2, 6);
    }

    private static OutboxProcessor BuildProcessor(IOutboxStore store, IInnerEventBus inner, OutboxOptions? options = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(store);
        services.AddSingleton(inner);
        var sp = services.BuildServiceProvider();

        return new OutboxProcessor(
            sp,
            new NeverSignalsNotifier(),
            new OutboxMetrics(),
            Microsoft.Extensions.Options.Options.Create(options ?? new OutboxOptions { PollingInterval = TimeSpan.FromMilliseconds(100) }),
            NullLogger<OutboxProcessor>.Instance);
    }

    private sealed class NeverSignalsNotifier : IOutboxNotifier
    {
        public ValueTask WaitForSignalAsync(CancellationToken cancellationToken)
            => new(Task.Delay(Timeout.Infinite, cancellationToken));
    }
}
