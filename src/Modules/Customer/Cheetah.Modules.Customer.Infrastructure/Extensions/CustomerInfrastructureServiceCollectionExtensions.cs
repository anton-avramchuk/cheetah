using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.Customer.Domain.Entities;
using Cheetah.Modules.Customer.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Customer.Infrastructure.Extensions;

public static class CustomerInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует инфраструктуру конкретной реализации Customer: DbContext (с клиентами и
    /// контактными лицами), мигратор, провайдер PostgreSQL и EF-репозитории
    /// <see cref="IRepository{TCustomer,Guid}"/> и <see cref="IRepository{TContact,Guid}"/>.
    /// Вызывается из инфраструктурного модуля наследника.
    /// </summary>
    public static IServiceCollection AddCustomerInfrastructure<TContext, TCustomer, TContact>(
        this IServiceCollection services)
        where TContext : CustomerDbContextBase<TContext, TCustomer, TContact>
        where TCustomer : CustomerBase
        where TContact : ContactBase
    {
        services.AddApplicationDbContext<TContext>();
        services.AddScoped<TContext>();
        services.AddDatabaseMigrator<TContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TContext>(); });
        services.AddScoped<IRepository<TCustomer, Guid>, EfRepository<TContext, TCustomer, Guid>>();
        services.AddScoped<IRepository<TContact, Guid>, EfRepository<TContext, TContact, Guid>>();
        return services;
    }
}
