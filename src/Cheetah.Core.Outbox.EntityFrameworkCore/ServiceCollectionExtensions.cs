using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Outbox.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует EfOutboxStore поверх указанного DbContext'a как реализацию IOutboxStore.
    /// </summary>
    public static IServiceCollection AddOutboxStore<TContext>(this IServiceCollection services)
        where TContext : DbContext, IOutboxDbContext
    {
        services.AddScoped<IOutboxStore, EfOutboxStore<TContext>>();
        return services;
    }

    /// <summary>
    /// Регистрирует EfInboxStore поверх указанного DbContext'a как реализацию IInboxStore.
    /// </summary>
    public static IServiceCollection AddInboxStore<TContext>(this IServiceCollection services)
        where TContext : DbContext, IInboxDbContext
    {
        services.AddScoped<IInboxStore, EfInboxStore<TContext>>();
        return services;
    }

    /// <summary>
    /// Регистрирует EfDeadLetterStore поверх указанного DbContext'a как реализацию IDeadLetterStore.
    /// </summary>
    public static IServiceCollection AddDeadLetterStore<TContext>(this IServiceCollection services)
        where TContext : DbContext, IDeadLetterDbContext
    {
        services.AddScoped<IDeadLetterStore, EfDeadLetterStore<TContext>>();
        return services;
    }
}
