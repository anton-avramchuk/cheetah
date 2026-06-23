using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Teams.Domain.Entities;
using Cheetah.Modules.Teams.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.Teams.Infrastructure.Extensions;

public static class TeamsInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует инфраструктуру конкретной реализации Teams: DbContext (команды + роли +
    /// участники + членства), мигратор, провайдер PostgreSQL и grid-репозитории
    /// (<see cref="IGridRepository{TEntity,TKey}"/>, реализующие также <see cref="IRepository{TEntity,TKey}"/>)
    /// для команды, ролей и участников. Вызывается из инфраструктурного модуля наследника.
    /// </summary>
    public static IServiceCollection AddTeamsInfrastructure<TContext, TTeam>(
        this IServiceCollection services)
        where TContext : TeamsDbContextBase<TContext, TTeam>
        where TTeam : TeamBase
    {
        services.AddApplicationDbContext<TContext>();
        services.AddScoped<TContext>();
        services.AddDatabaseMigrator<TContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TContext>(); });

        AddGridRepository<TContext, TTeam>(services);
        AddGridRepository<TContext, TeamRole>(services);
        AddGridRepository<TContext, TeamMember>(services);
        return services;
    }

    /// <summary>
    /// Регистрирует один grid-репозиторий и переиспользует его как обычный <see cref="IRepository{TEntity,Guid}"/>
    /// (EfGridRepository наследует EfRepository, поэтому хватает одного scoped-экземпляра на сущность).
    /// </summary>
    private static void AddGridRepository<TContext, TEntity>(IServiceCollection services)
        where TContext : Microsoft.EntityFrameworkCore.DbContext
        where TEntity : Cheetah.Core.Domain.Entity<Guid>
    {
        services.AddScoped<IGridRepository<TEntity, Guid>>(sp => new EfGridRepository<TContext, TEntity, Guid>(
            sp.GetRequiredService<TContext>(),
            sp.GetRequiredService<IObjectMapper>(),
            sp.GetRequiredService<ILoggerFactory>().CreateLogger($"Teams.Grid.{typeof(TEntity).Name}")));

        services.AddScoped<IRepository<TEntity, Guid>>(sp => sp.GetRequiredService<IGridRepository<TEntity, Guid>>());
    }
}
