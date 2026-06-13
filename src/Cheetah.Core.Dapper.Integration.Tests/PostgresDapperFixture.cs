using Cheetah.Core.Dapper.Connections;
using Cheetah.Core.Dapper.Dialect;
using Cheetah.Core.Dapper.Mapping;
using Cheetah.Core.Dapper.PostgreSql;
using Cheetah.Core.Dapper.Querying;
using Cheetah.Core.Dapper.Repositories;
using Cheetah.Core.Dapper.Specifications;
using Cheetah.Core.Dapper.Sql;
using Cheetah.Core.Dapper.UnitOfWork;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Cheetah.Core.Dapper.Integration.Tests;

/// <summary>
/// Spins up a PostgreSQL container, creates the <c>products</c> table, and wires the real Dapper
/// data-access services into a DI container — exercising SQL generation, Npgsql and transactions
/// end to end against a live database.
/// </summary>
public class PostgresDapperFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("dapper_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    private ServiceProvider _services = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            CREATE TABLE products (
                id        uuid    PRIMARY KEY,
                name      text    NOT NULL,
                price     numeric NOT NULL,
                stock     integer NOT NULL,
                is_active boolean NOT NULL
            );
            """);

        _services = BuildServiceProvider(ConnectionString);
    }

    public Task DisposeAsync()
    {
        _services.Dispose();
        return _container.DisposeAsync().AsTask();
    }

    /// <summary>Creates a fresh DI scope (new unit of work) per logical operation.</summary>
    public AsyncServiceScope CreateScope() => _services.CreateAsyncScope();

    /// <summary>Removes all rows so order-sensitive tests start from a clean table.</summary>
    public async Task TruncateAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("TRUNCATE TABLE products;");
    }

    /// <summary>Reads a single row directly (bypassing the repository) for assertions.</summary>
    public async Task<(string Name, decimal Price, int Stock, bool IsActive)?> ReadRawAsync(Guid id)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        var row = await connection.QueryFirstOrDefaultAsync(
            "SELECT name, price, stock, is_active FROM products WHERE id = @id", new { id });

        if (row is null)
            return null;

        return ((string)row.name, (decimal)row.price, (int)row.stock, (bool)row.is_active);
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConnectionStringResolver>(new StubConnectionStringResolver(connectionString));
        services.AddSingleton<IDapperConnectionProvider, NpgsqlConnectionProvider>();
        services.AddSingleton<ISqlDialect, PostgreSqlDialect>();
        services.AddSingleton<IDapperEntityMap, ProductMap>();
        services.AddSingleton<IEntityMapRegistry, EntityMapRegistry>();
        services.AddSingleton<ISqlBuilder, SqlBuilder>();
        services.AddSingleton<ISpecificationParser<SqlWhere>, ExpressionToSqlParser>();

        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<IDapperUnitOfWork, DapperUnitOfWork>();
        services.AddScoped<IDapperQueryExecutor, DapperQueryExecutor>();
        services.AddScoped<DapperRepository<Product, Guid>>();
        services.AddScoped<IRepository<Product, Guid>>(sp => sp.GetRequiredService<DapperRepository<Product, Guid>>());

        return services.BuildServiceProvider();
    }

    private sealed class StubConnectionStringResolver(string connectionString) : IConnectionStringResolver
    {
        public Task<string> ResolveAsync(string? connectionStringName = null) => Task.FromResult(connectionString);
    }
}

[CollectionDefinition("Dapper.Postgres")]
public class DapperPostgresCollection : ICollectionFixture<PostgresDapperFixture>;
