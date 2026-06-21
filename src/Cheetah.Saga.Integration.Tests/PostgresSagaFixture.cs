using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Cheetah.Saga.Integration.Tests;

public class PostgresSagaFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("saga_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public SagaTestDbContext CreateDbContext()
    {
        var opts = new DbContextOptionsBuilder<SagaTestDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new SagaTestDbContext(opts);
    }
}

[CollectionDefinition("Saga")]
public class SagaCollection : ICollectionFixture<PostgresSagaFixture> { }
