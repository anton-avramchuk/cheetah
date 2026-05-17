using Cheetah.Core.Outbox.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Cheetah.Core.Outbox.Integration.Tests;

/// <summary>
/// Поднимает Postgres-контейнер на класс тестов, создаёт схему и триггер outbox_new.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("outbox_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        // Триггер pg_notify
        await db.Database.ExecuteSqlRawAsync(OutboxNotifyTriggerSql.Create());
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new TestDbContext(options);
    }
}
