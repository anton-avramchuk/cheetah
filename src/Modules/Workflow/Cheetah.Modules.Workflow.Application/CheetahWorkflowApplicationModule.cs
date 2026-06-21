using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Expressions.JsonLogic;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Modules.Workflow.Domain;
using Cheetah.Modules.Workflow.DomainEvents;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Application;

/// <summary>
/// Прикладной слой Workflow: движок правил (<see cref="IWorkflowEngine"/>), рендер параметров действий и
/// generic CQRS правил. Закрытые generic-handler'ы регистрирует наследник через
/// <c>AddWorkflowApplication&lt;…&gt;()</c>.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmExpressionsJsonLogicModule),
    typeof(CrmWorkflowModule),
    typeof(CheetahWorkflowDomainModule),
    typeof(CheetahWorkflowContractsModule),
    typeof(CheetahWorkflowDomainEventsModule))]
public partial class CheetahWorkflowApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
