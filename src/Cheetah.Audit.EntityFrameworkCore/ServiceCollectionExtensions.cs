using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Audit.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует EfAuditSink&lt;TContext&gt; как IAuditSink. Не забудьте подключить
    /// AuditInterceptor к DbContext'у через AddInterceptors(...).
    /// </summary>
    public static IServiceCollection AddEfAuditSink<TContext>(this IServiceCollection services)
        where TContext : DbContext, IAuditDbContext
    {
        services.AddScoped<IAuditSink, EfAuditSink<TContext>>();
        return services;
    }

    /// <summary>
    /// Регистрирует EfAuditPublishStore — используется KafkaAuditPublisher для чтения unpublished.
    /// </summary>
    public static IServiceCollection AddEfAuditPublishStore<TContext>(this IServiceCollection services)
        where TContext : DbContext, IAuditDbContext
    {
        services.AddScoped<IAuditPublishStore, EfAuditPublishStore<TContext>>();
        return services;
    }
}
