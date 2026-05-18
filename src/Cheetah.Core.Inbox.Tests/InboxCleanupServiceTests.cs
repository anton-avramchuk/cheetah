using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Cheetah.Core.Inbox.Tests;

public class InboxCleanupServiceTests
{
    [Fact]
    public async Task RunOnce_Deletes_In_Batches_Until_Store_Returns_Zero()
    {
        var inbox = new Mock<IInboxStore>();
        var calls = 0;
        inbox.Setup(s => s.DeleteOlderThanAsync(It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns<DateTimeOffset, int, CancellationToken>((_, _, _) =>
            {
                calls++;
                // 3 батча по 100, потом пусто
                return new ValueTask<int>(calls <= 3 ? 100 : 0);
            });

        var services = new ServiceCollection();
        services.AddSingleton(inbox.Object);

        var sut = new InboxCleanupService(
            services.BuildServiceProvider(),
            new FakeInboxMetrics(),
            Microsoft.Extensions.Options.Options.Create(new InboxOptions
            {
                CleanupInterval = TimeSpan.FromMilliseconds(100),
                RetentionPeriod = TimeSpan.FromDays(1),
                CleanupBatchSize = 100
            }),
            NullLogger<InboxCleanupService>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        await sut.StartAsync(cts.Token);
        await Task.Delay(300);
        await sut.StopAsync(CancellationToken.None);

        // 3 ненулевых вызова + 1 нулевой = 4 минимум
        inbox.Verify(s => s.DeleteOlderThanAsync(It.IsAny<DateTimeOffset>(), 100, It.IsAny<CancellationToken>()),
            Times.AtLeast(4));
    }

    [Fact]
    public async Task If_IInboxStore_Not_Registered_Service_Does_Nothing()
    {
        var services = new ServiceCollection();
        // IInboxStore намеренно НЕ регистрируем

        var sut = new InboxCleanupService(
            services.BuildServiceProvider(),
            new FakeInboxMetrics(),
            Microsoft.Extensions.Options.Options.Create(new InboxOptions
            {
                CleanupInterval = TimeSpan.FromMilliseconds(100)
            }),
            NullLogger<InboxCleanupService>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        await sut.StartAsync(cts.Token);
        await Task.Delay(200);
        await sut.StopAsync(CancellationToken.None);

        // Не должен упасть. Других проверок нет — отсутствие IInboxStore приводит к no-op.
    }

    [Fact]
    public async Task Cleanup_Uses_Configured_Retention_Threshold()
    {
        DateTimeOffset? capturedThreshold = null;
        var inbox = new Mock<IInboxStore>();
        inbox.Setup(s => s.DeleteOlderThanAsync(It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns<DateTimeOffset, int, CancellationToken>((th, _, _) =>
            {
                capturedThreshold ??= th;
                return new ValueTask<int>(0);
            });

        var services = new ServiceCollection();
        services.AddSingleton(inbox.Object);

        var retention = TimeSpan.FromDays(7);
        var sut = new InboxCleanupService(
            services.BuildServiceProvider(),
            new FakeInboxMetrics(),
            Microsoft.Extensions.Options.Options.Create(new InboxOptions
            {
                CleanupInterval = TimeSpan.FromMilliseconds(100),
                RetentionPeriod = retention
            }),
            NullLogger<InboxCleanupService>.Instance);

        var before = DateTimeOffset.UtcNow;
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        await sut.StartAsync(cts.Token);
        await Task.Delay(200);
        await sut.StopAsync(CancellationToken.None);

        capturedThreshold.ShouldNotBeNull();
        // threshold = now - retention; должно попасть в окно [before-retention, after-retention]
        var after = DateTimeOffset.UtcNow;
        capturedThreshold.Value.ShouldBeGreaterThanOrEqualTo(before - retention);
        capturedThreshold.Value.ShouldBeLessThanOrEqualTo(after - retention);
    }

    private sealed class FakeInboxMetrics : IInboxMetrics
    {
        public int CleanedTotal { get; private set; }
        public void RecordDuplicateSkipped(string consumerName) { }
        public void RecordProcessed(string consumerName) { }
        public void RecordCleaned(int count) => CleanedTotal += count;
    }
}
