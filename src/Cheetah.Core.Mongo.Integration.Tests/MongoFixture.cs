using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Inbox;
using Cheetah.Core.Inbox.Mongo;
using Cheetah.Core.Mongo.Connections;
using Cheetah.Core.Mongo.Mapping;
using Cheetah.Core.Mongo.Specifications;
using Cheetah.Core.Mongo.UnitOfWork;
using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.Mongo;
using Cheetah.Saga;
using Cheetah.Saga.Mongo;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Cheetah.Core.Mongo.Integration.Tests;

/// <summary>
/// Spins up a single-node MongoDB replica set (required for multi-document transactions) and wires
/// the real Mongo data-access services into a DI container. Services are registered by hand (no
/// module bootstrap), the same approach the Dapper integration fixture uses.
/// </summary>
public class MongoFixture : IAsyncLifetime
{
    private const string DatabaseName = "cheetah_test";

    private readonly MongoDbContainer _container = new MongoDbBuilder()
        .WithImage("mongo:7")
        .WithReplicaSet()
        .Build();

    private ServiceProvider _services = null!;

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // The replica-set container uses authentication (user mongo/mongo in the admin db). Keep the
        // container's credentials but: (1) set our database in the path, (2) pin authSource=admin —
        // otherwise the driver would authenticate against our database and SCRAM fails — and (3) use
        // directConnection=true to talk straight to the single replica-set node (its internal
        // hostname is unreachable from the host) while still reporting a replica-set topology, so
        // multi-document transactions are allowed.
        var builder = new MongoUrlBuilder(_container.GetConnectionString())
        {
            DatabaseName = DatabaseName,
            AuthenticationSource = "admin",
            DirectConnection = true,
        };
        ConnectionString = builder.ToString();

        MongoMappingConventions.EnsureRegistered();
        _services = BuildServiceProvider(ConnectionString);
    }

    public Task DisposeAsync()
    {
        _services.Dispose();
        return _container.DisposeAsync().AsTask();
    }

    public AsyncServiceScope CreateScope() => _services.CreateAsyncScope();

    public IMongoDatabase Database
        => new MongoClient(ConnectionString).GetDatabase(DatabaseName);

    public async Task DropAllAsync()
    {
        var db = Database;
        using var cursor = await db.ListCollectionNamesAsync();
        foreach (var name in await cursor.ToListAsync())
            await db.DropCollectionAsync(name);
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConnectionStringResolver>(new StubConnectionStringResolver(connectionString));
        services.AddSingleton<IMongoClientProvider, MongoClientProvider>();
        services.AddSingleton<IMongoEntityMapRegistry>(new MongoEntityMapRegistry(Array.Empty<IMongoEntityMap>()));
        services.AddSingleton<IMongoFilterBuilder, MongoFilterBuilder>();

        services.AddScoped<IMongoDatabaseProvider, MongoDatabaseProvider>();
        services.AddScoped<IMongoCollectionResolver, MongoCollectionResolver>();
        services.AddScoped<IMongoUnitOfWork, MongoUnitOfWork>();
        services.AddScoped<Repositories.MongoRepository<Product, Guid>>();
        services.AddScoped<IRepository<Product, Guid>>(sp => sp.GetRequiredService<Repositories.MongoRepository<Product, Guid>>());

        // Stores under test (auto-registered via [Export] in the real app; wired by hand here).
        services.AddOptions<MongoSagaStoreOptions>();
        services.AddOptions<MongoInboxStoreOptions>();
        services.AddOptions<MongoOutboxStoreOptions>();
        services.AddScoped<ISagaRepository, MongoSagaRepository>();
        services.AddScoped<IInboxStore, MongoInboxStore>();
        services.AddScoped<IOutboxStore, MongoOutboxStore>();
        services.AddScoped<IDeadLetterStore, MongoDeadLetterStore>();

        return services.BuildServiceProvider();
    }

    private sealed class StubConnectionStringResolver(string connectionString) : IConnectionStringResolver
    {
        public Task<string> ResolveAsync(string? connectionStringName = null) => Task.FromResult(connectionString);
    }
}

[CollectionDefinition("Mongo")]
public class MongoCollection : ICollectionFixture<MongoFixture>;
