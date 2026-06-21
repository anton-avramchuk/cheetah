using Cheetah.Core;
using Cheetah.Core.Cache;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Workflow.Application;
using Cheetah.Modules.Workflow.Domain;
using Cheetah.Modules.Workflow.DomainEvents;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля Workflow: абстрактные базы EF
/// (<see cref="Persistence.WorkflowDbContextBase{TContext,TRule}"/>,
/// <see cref="Persistence.Configurations.AutomationRuleConfigurationBase{TRule}"/>), child-aware
/// репозиторий, матчинг через БД и in-proc исполнитель действий. Generic-регистрация —
/// <c>AddWorkflowInfrastructure&lt;TContext,TRule&gt;()</c>. Конкретный DbContext и миграции — в <c>.Default</c>.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmCacheCoreModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmWorkflowModule),
    typeof(CheetahWorkflowDomainModule),
    typeof(CheetahWorkflowApplicationModule),
    typeof(CheetahWorkflowDomainEventsModule))]
public partial class CheetahWorkflowInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
