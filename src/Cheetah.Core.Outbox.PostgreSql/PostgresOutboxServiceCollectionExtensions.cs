using Cheetah.Core.Outbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Outbox.PostgreSql;

public static class PostgresOutboxServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует PostgresOutboxStore (SKIP LOCKED) как IOutboxStore поверх указанного DbContext'a.
    /// Используй вместо AddOutboxStore&lt;TContext&gt;() в Postgres-проектах с несколькими репликами.
    /// </summary>
    public static IServiceCollection AddPostgresOutboxStore<TContext>(this IServiceCollection services)
        where TContext : DbContext, IOutboxDbContext
    {
        services.AddScoped<IOutboxStore, PostgresOutboxStore<TContext>>();
        return services;
    }
}
