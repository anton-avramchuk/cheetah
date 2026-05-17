using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Saga.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует EfSagaRepository&lt;TContext&gt; как ISagaRepository.
    /// </summary>
    public static IServiceCollection AddEfSagaRepository<TContext>(this IServiceCollection services)
        where TContext : DbContext, ISagaDbContext
    {
        services.AddScoped<ISagaRepository, EfSagaRepository<TContext>>();
        return services;
    }
}
