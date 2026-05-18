using Cheetah.Core.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Cheetah.Core.Outbox.Tests;

public class OutboxCleanupServiceTests
{
    [Fact]
    public async Task RunOnce_Deletes_In_Batches_Until_Store_Returns_Zero()
    {
        var outbox = new Mock<IOutboxStore>();
        var calls = 0;
        outbox.Setup(s => s.DeleteProcessedAsync(It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns<DateTimeOffset, int, CancellationToken>((_, _, _) =>
            {
                calls++;
                // 3 батча по 100, потом пусто
                return new ValueTask<int>(calls <= 3 ? 100 : 0);
            });

        var services = new ServiceCollection();
        services.AddSingleton(outbox.Object);

        var sut = new OutboxCleanupService(
            services.BuildServiceProvider(),
            new OutboxProcessorTests.FakeMetrics(),
            Microsoft.Extensions.Options.Options.Create(new OutboxOptions
            {
                CleanupInterval = TimeSpan.FromMilliseconds(100),
                RetentionPeriod = TimeSpan.FromDays(1),
                CleanupBatchSize = 100
            }),
            NullLogger<OutboxCleanupService>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        await sut.StartAsync(cts.Token);
        await Task.Delay(300);
        await sut.StopAsync(CancellationToken.None);

        // 3 ненулевых вызова + 1 нулевой = 4 минимум
        outbox.Verify(s => s.DeleteProcessedAsync(It.IsAny<DateTimeOffset>(), 100, It.IsAny<CancellationToken>()),
            Times.AtLeast(4));
    }
}
