using Cheetah.Core.Dapper.Repositories;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Dapper.Extensions;

/// <summary>
/// Helpers for binding repository abstractions to the Dapper implementation per entity.
/// Binding is opt-in so Dapper does not silently replace EF Core repositories.
/// </summary>
public static class DapperServiceCollectionExtensions
{
    /// <summary>
    /// Binds <see cref="IRepository{TEntity,TKey}"/> and <see cref="IReadOnlyRepository{TEntity,TKey}"/>
    /// for <typeparamref name="TEntity"/> to the Dapper implementation.
    /// </summary>
    public static IServiceCollection AddDapperRepository<TEntity, TKey>(this IServiceCollection services)
        where TEntity : Entity<TKey>
    {
        services.AddScoped<IRepository<TEntity, TKey>, DapperRepository<TEntity, TKey>>();
        services.AddScoped<IReadOnlyRepository<TEntity, TKey>>(sp => sp.GetRequiredService<IRepository<TEntity, TKey>>());
        return services;
    }

    /// <summary>
    /// Binds the <see cref="Guid"/>-keyed repository abstractions for <typeparamref name="TEntity"/>
    /// to the Dapper implementation.
    /// </summary>
    public static IServiceCollection AddDapperRepository<TEntity>(this IServiceCollection services)
        where TEntity : Entity<Guid>
    {
        services.AddScoped<IRepository<TEntity>, DapperRepository<TEntity>>();
        services.AddScoped<IRepository<TEntity, Guid>>(sp => sp.GetRequiredService<IRepository<TEntity>>());
        services.AddScoped<IReadOnlyRepository<TEntity>>(sp => sp.GetRequiredService<IRepository<TEntity>>());
        services.AddScoped<IReadOnlyRepository<TEntity, Guid>>(sp => sp.GetRequiredService<IRepository<TEntity>>());
        return services;
    }
}
