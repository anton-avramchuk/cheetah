using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Cheetah.Modules.SalesDocuments.Domain.Abstractions;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Infrastructure.Persistence;
using Cheetah.Modules.SalesDocuments.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.SalesDocuments.Infrastructure.Extensions;

public static class SalesDocumentsInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует инфраструктуру конкретной реализации SalesDocuments: DbContext, мигратор,
    /// провайдер PostgreSQL, grid-репозиторий документа (он же <see cref="IRepository{TEntity,TKey}"/>),
    /// генератор номеров, PDF-сервис и дефолтный (пустой) адаптер цен. Вызывается из инфраструктурного
    /// модуля наследника. Реальный адаптер каталога заменяет <see cref="IProductPricingPort"/> позже.
    /// </summary>
    public static IServiceCollection AddSalesDocumentsInfrastructure<TContext, TDoc>(
        this IServiceCollection services)
        where TContext : SalesDocumentsDbContextBase<TContext, TDoc>
        where TDoc : SalesDocumentBase
    {
        services.AddApplicationDbContext<TContext>();
        services.AddScoped<TContext>();
        services.AddDatabaseMigrator<TContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TContext>(); });

        AddGridRepository<TContext, TDoc>(services);

        services.AddScoped<IDocumentNumberGenerator, SequentialDocumentNumberGenerator<TContext, TDoc>>();
        services.AddScoped<IDocumentPdfService, FileStorageDocumentPdfService>();
        services.TryAddProductPricingPort<NullProductPricingPort>();

        return services;
    }

    /// <summary>Регистрирует адаптер цен каталога, если он ещё не зарегистрирован.</summary>
    private static IServiceCollection TryAddProductPricingPort<TPort>(this IServiceCollection services)
        where TPort : class, IProductPricingPort
    {
        if (services.All(d => d.ServiceType != typeof(IProductPricingPort)))
            services.AddScoped<IProductPricingPort, TPort>();
        return services;
    }

    private static void AddGridRepository<TContext, TEntity>(IServiceCollection services)
        where TContext : Microsoft.EntityFrameworkCore.DbContext
        where TEntity : Cheetah.Core.Domain.Entity<Guid>
    {
        services.AddScoped<IGridRepository<TEntity, Guid>>(sp => new EfGridRepository<TContext, TEntity, Guid>(
            sp.GetRequiredService<TContext>(),
            sp.GetRequiredService<IObjectMapper>(),
            sp.GetRequiredService<ILoggerFactory>().CreateLogger($"SalesDocuments.Grid.{typeof(TEntity).Name}")));

        services.AddScoped<IRepository<TEntity, Guid>>(sp => sp.GetRequiredService<IGridRepository<TEntity, Guid>>());
    }
}
