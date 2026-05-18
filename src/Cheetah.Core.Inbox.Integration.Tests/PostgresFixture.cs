using Cheetah.Core.Inbox.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Cheetah.Core.Inbox.Integration.Tests;

/// <summary>
/// Поднимает Postgres-контейнер на класс тестов, создаёт схему inbox и оптимизированный индекс.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("inbox_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        // Postgres-оптимизированные индексы
        await db.Database.ExecuteSqlRawAsync(InboxOptimizedIndexSql.Create());
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public InboxTestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InboxTestDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new InboxTestDbContext(options);
    }
}
