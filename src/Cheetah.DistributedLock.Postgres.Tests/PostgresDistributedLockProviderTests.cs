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
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
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
    public async Task TryAcquire_первая_попытка_получает_lock()
    {
        var key = $"test-{Guid.NewGuid()}";
        await using var lockHandle = await _sut.TryAcquireAsync(key);

        lockHandle.ShouldNotBeNull();
        lockHandle!.Key.ShouldBe(key);
    }

    [Fact]
    public async Task TryAcquire_второй_получает_null_пока_первый_удерживает()
    {
        var key = $"contended-{Guid.NewGuid()}";

        await using var first = await _sut.TryAcquireAsync(key);
        first.ShouldNotBeNull();

        var second = await _sut.TryAcquireAsync(key);
        second.ShouldBeNull();
    }

    [Fact]
    public async Task Dispose_освобождает_lock_и_следующий_TryAcquire_получает_его()
    {
        var key = $"release-{Guid.NewGuid()}";

        var first = await _sut.TryAcquireAsync(key);
        first.ShouldNotBeNull();
        await first!.DisposeAsync();

        await using var second = await _sut.TryAcquireAsync(key);
        second.ShouldNotBeNull();
    }

    [Fact]
    public async Task AcquireAsync_с_timeout_бросает_DistributedLockTimeoutException_при_занятости()
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
    public async Task AcquireAsync_ждёт_и_получает_после_освобождения()
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
    public async Task Разные_ключи_не_конкурируют()
    {
        var keyA = $"a-{Guid.NewGuid()}";
        var keyB = $"b-{Guid.NewGuid()}";

        await using var lockA = await _sut.TryAcquireAsync(keyA);
        await using var lockB = await _sut.TryAcquireAsync(keyB);

        lockA.ShouldNotBeNull();
        lockB.ShouldNotBeNull();
    }

    [Fact]
    public async Task Параллельные_TryAcquire_только_один_получает_lock()
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
