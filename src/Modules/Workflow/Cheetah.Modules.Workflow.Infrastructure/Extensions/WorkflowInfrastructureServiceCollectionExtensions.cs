using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.Workflow.Application;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Domain.Repositories;
using Cheetah.Modules.Workflow.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Workflow.Infrastructure.Extensions;

public static class WorkflowInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует БД-инфраструктуру конкретной реализации Workflow: DbContext (правила + журнал),
    /// мигратор, провайдер PostgreSQL, child-aware репозиторий правил, репозиторий журнала и матчинг.
    /// Вызывается из инфраструктурного модуля наследника / <c>.Default</c>.
    /// </summary>
    public static IServiceCollection AddWorkflowInfrastructure<TContext, TRule>(this IServiceCollection services)
        where TContext : WorkflowDbContextBase<TContext, TRule>
        where TRule : AutomationRuleBase
    {
        services.AddApplicationDbContext<TContext>();
        services.AddScoped<TContext>();
        services.AddDatabaseMigrator<TContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TContext>(); });

        // Child-aware репозиторий правил; интерфейсы резолвятся в один экземпляр (общий DbContext-scope).
        services.AddScoped<AutomationRuleRepository<TContext, TRule>>();
        services.AddScoped<IAutomationRuleRepository<TRule>>(sp => sp.GetRequiredService<AutomationRuleRepository<TContext, TRule>>());
        services.AddScoped<IRepository<TRule, Guid>>(sp => sp.GetRequiredService<AutomationRuleRepository<TContext, TRule>>());
        services.AddScoped<IReadOnlyRepository<TRule, Guid>>(sp => sp.GetRequiredService<AutomationRuleRepository<TContext, TRule>>());

        // Журнал срабатываний — общий EF-репозиторий.
        services.AddScoped<EfRepository<TContext, AutomationRun, Guid>>();
        services.AddScoped<IRepository<AutomationRun, Guid>>(sp => sp.GetRequiredService<EfRepository<TContext, AutomationRun, Guid>>());
        services.AddScoped<IReadOnlyRepository<AutomationRun, Guid>>(sp => sp.GetRequiredService<EfRepository<TContext, AutomationRun, Guid>>());

        // Матчинг event → активные правила (кэш-индекс — follow-up).
        services.AddScoped<IRuleMatcher<TRule>, DbQueryRuleMatcher<TRule>>();

        return services;
    }
}
