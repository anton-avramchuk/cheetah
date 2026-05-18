using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Inbox.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует EfInboxStore поверх указанного DbContext'a как реализацию IInboxStore.
    /// </summary>
    public static IServiceCollection AddInboxStore<TContext>(this IServiceCollection services)
        where TContext : DbContext, IInboxDbContext
    {
        services.AddScoped<IInboxStore, EfInboxStore<TContext>>();
        return services;
    }
}
