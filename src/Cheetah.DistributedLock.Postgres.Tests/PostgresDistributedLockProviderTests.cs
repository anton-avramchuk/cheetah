using Cheetah.DistributedLock;
using Cheetah.DistributedLock.Postgres;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using Testcontainers.PostgreSql;

namespace Cheetah.DistributedLock.Postgres.Tests;

public class PostgresDistributedLockProviderTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("lock_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private PostgresDistributedLockProvider _sut = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _sut = new PostgresDistributedLockProvider(
            Options.Create(new PostgresLockOptions
            {
                ConnectionString = _container.GetConnectionString(),
                RetryInterval = TimeSpan.FromMilliseconds(50)
            }),
            new ConfigurationBuilder().Build(),
            NullLogger<PostgresDistributedLockProvider>.Instance);
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task TryAcquire_First_Attempt_Acquires_Lock()
    {
        var key = $"test-{Guid.NewGuid()}";
        await using var lockHandle = await _sut.TryAcquireAsync(key);

        lockHandle.ShouldNotBeNull();
        lockHandle!.Key.ShouldBe(key);
    }

    [Fact]
    public async Task TryAcquire_Second_Returns_Null_While_First_Holds()
    {
        var key = $"contended-{Guid.NewGuid()}";

        await using var first = await _sut.TryAcquireAsync(key);
        first.ShouldNotBeNull();

        var second = await _sut.TryAcquireAsync(key);
        second.ShouldBeNull();
    }

    [Fact]
    public async Task Dispose_Releases_Lock_And_Next_TryAcquire_Acquires_It()
    {
        var key = $"release-{Guid.NewGuid()}";

        var first = await _sut.TryAcquireAsync(key);
        first.ShouldNotBeNull();
        await first!.DisposeAsync();

        await using var second = await _sut.TryAcquireAsync(key);
        second.ShouldNotBeNull();
    }

    [Fact]
    public async Task AcquireAsync_With_Timeout_Throws_DistributedLockTimeoutException_When_Held()
    {
        var key = $"timeout-{Guid.NewGuid()}";

        await using var holder = await _sut.TryAcquireAsync(key);
        holder.ShouldNotBeNull();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var ex = await Should.ThrowAsync<DistributedLockTimeoutException>(
            () => _sut.AcquireAsync(key, TimeSpan.FromMilliseconds(300)).AsTask());
        sw.Stop();

        ex.Key.ShouldBe(key);
        sw.ElapsedMilliseconds.ShouldBeInRange(250, 1000);
    }

    [Fact]
    public async Task AcquireAsync_Waits_And_Acquires_After_Release()
    {
        var key = $"wait-{Guid.NewGuid()}";

        var first = await _sut.TryAcquireAsync(key);
        first.ShouldNotBeNull();

        // Освобождаем через 200ms
        var releaseTask = Task.Run(async () =>
        {
            await Task.Delay(200);
            await first!.DisposeAsync();
        });

        await using var second = await _sut.AcquireAsync(key, TimeSpan.FromSeconds(5));
        second.ShouldNotBeNull();
        await releaseTask;
    }

    [Fact]
    public async Task Different_Keys_Do_Not_Contend()
    {
        var keyA = $"a-{Guid.NewGuid()}";
        var keyB = $"b-{Guid.NewGuid()}";

        await using var lockA = await _sut.TryAcquireAsync(keyA);
        await using var lockB = await _sut.TryAcquireAsync(keyB);

        lockA.ShouldNotBeNull();
        lockB.ShouldNotBeNull();
    }

    [Fact]
    public async Task Parallel_TryAcquire_Only_One_Acquires_Lock()
    {
        var key = $"race-{Guid.NewGuid()}";

        var attempts = Enumerable.Range(0, 10)
            .Select(_ => _sut.TryAcquireAsync(key).AsTask())
            .ToArray();
        var results = await Task.WhenAll(attempts);

        var successful = results.Count(x => x is not null);
        successful.ShouldBe(1);

        // Cleanup
        foreach (var l in results)
        {
            if (l is not null) await l.DisposeAsync();
        }
    }
}
