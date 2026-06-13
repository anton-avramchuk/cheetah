using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Domain;
using Cheetah.Core.Mongo.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Mongo.Extensions;

/// <summary>
/// Helpers for binding repository abstractions to the MongoDB implementation per entity. Binding is
/// opt-in so Mongo does not silently replace EF Core or Dapper repositories.
/// </summary>
public static class MongoServiceCollectionExtensions
{
    /// <summary>
    /// Binds <see cref="IRepository{TEntity,TKey}"/> and <see cref="IReadOnlyRepository{TEntity,TKey}"/>
    /// for <typeparamref name="TEntity"/> to the MongoDB implementation.
    /// </summary>
    public static IServiceCollection AddMongoRepository<TEntity, TKey>(this IServiceCollection services)
        where TEntity : Entity<TKey>
    {
        services.AddScoped<IRepository<TEntity, TKey>, MongoRepository<TEntity, TKey>>();
        services.AddScoped<IReadOnlyRepository<TEntity, TKey>>(sp => sp.GetRequiredService<IRepository<TEntity, TKey>>());
        return services;
    }

    /// <summary>
    /// Binds the <see cref="Guid"/>-keyed repository abstractions for <typeparamref name="TEntity"/>
    /// to the MongoDB implementation.
    /// </summary>
    public static IServiceCollection AddMongoRepository<TEntity>(this IServiceCollection services)
        where TEntity : Entity<Guid>
    {
        services.AddScoped<IRepository<TEntity>, MongoRepository<TEntity>>();
        services.AddScoped<IRepository<TEntity, Guid>>(sp => sp.GetRequiredService<IRepository<TEntity>>());
        services.AddScoped<IReadOnlyRepository<TEntity>>(sp => sp.GetRequiredService<IRepository<TEntity>>());
        services.AddScoped<IReadOnlyRepository<TEntity, Guid>>(sp => sp.GetRequiredService<IRepository<TEntity>>());
        return services;
    }
}
