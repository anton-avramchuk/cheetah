using Cheetah.Core.Events;
using Cheetah.Core.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Cheetah.Core.Outbox.Tests;

public class DeadLetterProcessorTests
{
    [Fact]
    public async Task При_превышении_MaxRetries_сообщение_едет_в_DLQ_а_не_в_MarkFailed()
    {
        var msg = OutboxEventSerializer.Serialize(new TestEvent("dead"));
        msg.RetryCount = 10; // RetryCount + 1 = 11 > MaxRetries(10)

        var store = new Mock<IOutboxStore>();
        var first = true;
        store.Setup(s => s.GetPendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() => first
                ? new ValueTask<IReadOnlyList<OutboxMessage>>(new[] { msg })
                : new ValueTask<IReadOnlyList<OutboxMessage>>(Array.Empty<OutboxMessage>()))
            .Callback(() => first = false);

        var dlq = new Mock<IDeadLetterStore>();
        dlq.Setup(d => d.MoveFromOutboxAsync(It.IsAny<OutboxMessage>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var inner = new Mock<IInnerEventBus>();
        inner.Setup(b => b.PublishAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException("permanent failure"));

        var services = new ServiceCollection();
        services.AddSingleton(store.Object);
        services.AddSingleton(inner.Object);
        services.AddSingleton(dlq.Object);

        var sut = new OutboxProcessor(
            services.BuildServiceProvider(),
            new NeverSignals(),
            new OutboxProcessorTests.FakeMetrics(),
            Microsoft.Extensions.Options.Options.Create(new OutboxOptions
            {
                MaxRetries = 10,
                PollingInterval = TimeSpan.FromMilliseconds(100)
            }),
            NullLogger<OutboxProcessor>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await sut.StartAsync(cts.Token);
        await Task.Delay(400);
        await sut.StopAsync(CancellationToken.None);

        dlq.Verify(d => d.MoveFromOutboxAsync(
                It.Is<OutboxMessage>(m => m.Id == msg.Id),
                It.Is<string>(e => e.Contains("permanent failure")),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        store.Verify(s => s.MarkFailedAsync(msg.Id, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Если_DLQ_не_зарегистрирован_сообщение_продолжает_retry()
    {
        var msg = OutboxEventSerializer.Serialize(new TestEvent("ret"));
        msg.RetryCount = 100; // даже выше MaxRetries

        var store = new Mock<IOutboxStore>();
        var first = true;
        store.Setup(s => s.GetPendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() => first
                ? new ValueTask<IReadOnlyList<OutboxMessage>>(new[] { msg })
                : new ValueTask<IReadOnlyList<OutboxMessage>>(Array.Empty<OutboxMessage>()))
            .Callback(() => first = false);
        store.Setup(s => s.MarkFailedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var inner = new Mock<IInnerEventBus>();
        inner.Setup(b => b.PublishAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Throws(new InvalidOperationException("err"));

        var services = new ServiceCollection();
        services.AddSingleton(store.Object);
        services.AddSingleton(inner.Object);
        // IDeadLetterStore намеренно не зарегистрирован

        var sut = new OutboxProcessor(
            services.BuildServiceProvider(),
            new NeverSignals(),
            new OutboxProcessorTests.FakeMetrics(),
            Microsoft.Extensions.Options.Options.Create(new OutboxOptions
            {
                MaxRetries = 10,
                PollingInterval = TimeSpan.FromMilliseconds(100)
            }),
            NullLogger<OutboxProcessor>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await sut.StartAsync(cts.Token);
        await Task.Delay(400);
        await sut.StopAsync(CancellationToken.None);

        store.Verify(s => s.MarkFailedAsync(msg.Id, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    private sealed class NeverSignals : IOutboxNotifier
    {
        public ValueTask WaitForSignalAsync(CancellationToken ct)
            => new(Task.Delay(Timeout.Infinite, ct));
    }
}
