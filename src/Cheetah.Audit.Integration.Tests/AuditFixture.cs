using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;

namespace Cheetah.Audit.Integration.Tests;

/// <summary>
/// Поднимает Postgres + Kafka в Testcontainers. Один экземпляр на класс тестов
/// (через ICollectionFixture на всю сборку).
/// </summary>
public class AuditFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("audit_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly KafkaContainer _kafka = new KafkaBuilder("confluentinc/cp-kafka:7.6.1")
        .Build();

    public string PostgresConnectionString => _postgres.GetConnectionString();
    public string KafkaBootstrapServers => _kafka.GetBootstrapAddress();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _kafka.StartAsync());

        await using var db = CreateDbContext(withInterceptor: null);
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
        => Task.WhenAll(_postgres.DisposeAsync().AsTask(), _kafka.DisposeAsync().AsTask());

    public AuditDbContext CreateDbContext(AuditInterceptor? withInterceptor)
    {
        var builder = new DbContextOptionsBuilder<AuditDbContext>().UseNpgsql(PostgresConnectionString);
        if (withInterceptor is not null) builder.AddInterceptors(withInterceptor);
        return new AuditDbContext(builder.Options);
    }
}

[CollectionDefinition("Audit")]
public class AuditCollection : ICollectionFixture<AuditFixture> { }
