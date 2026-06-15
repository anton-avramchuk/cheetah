using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.Activities.Domain.Entities;
using Cheetah.Modules.Activities.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Activities.Infrastructure.Extensions;

public static class ActivitiesInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует инфраструктуру конкретной реализации Activities: DbContext (активности +
    /// напоминания), мигратор, провайдер PostgreSQL и EF-репозитории
    /// <see cref="IRepository{TActivity,Guid}"/> и <see cref="IRepository{ActivityReminder,Guid}"/>.
    /// Вызывается из инфраструктурного модуля наследника.
    /// </summary>
    public static IServiceCollection AddActivitiesInfrastructure<TContext, TActivity>(
        this IServiceCollection services)
        where TContext : ActivitiesDbContextBase<TContext, TActivity>
        where TActivity : ActivityBase
    {
        services.AddApplicationDbContext<TContext>();
        services.AddScoped<TContext>();
        services.AddDatabaseMigrator<TContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TContext>(); });
        services.AddScoped<IRepository<TActivity, Guid>, EfRepository<TContext, TActivity, Guid>>();
        services.AddScoped<IRepository<ActivityReminder, Guid>, EfRepository<TContext, ActivityReminder, Guid>>();
        return services;
    }
}
