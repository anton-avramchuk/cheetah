using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Leads.Infrastructure.Extensions;

public static class LeadsInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует инфраструктуру конкретной реализации Leads: DbContext, мигратор, провайдер
    /// PostgreSQL и EF-репозиторий <see cref="IRepository{TLead,Guid}"/>. Вызывается из
    /// инфраструктурного модуля наследника.
    /// </summary>
    public static IServiceCollection AddLeadsInfrastructure<TContext, TLead>(
        this IServiceCollection services)
        where TContext : LeadsDbContextBase<TContext, TLead>
        where TLead : LeadBase
    {
        services.AddApplicationDbContext<TContext>();
        services.AddScoped<TContext>();
        services.AddDatabaseMigrator<TContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TContext>(); });
        services.AddScoped<IRepository<TLead, Guid>, EfRepository<TContext, TLead, Guid>>();
        return services;
    }
}
