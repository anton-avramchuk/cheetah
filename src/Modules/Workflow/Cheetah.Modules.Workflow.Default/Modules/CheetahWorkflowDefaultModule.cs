using Cheetah.Core;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Events;
using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Workflow.Api;
using Cheetah.Modules.Workflow.Application;
using Cheetah.Modules.Workflow.Domain;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Modules.Workflow.Default.Contracts;
using Cheetah.Modules.Workflow.Default.Endpoints;
using Cheetah.Modules.Workflow.Default.Entities;
using Cheetah.Modules.Workflow.Default.Factories;
using Cheetah.Modules.Workflow.Default.Persistence;
using Cheetah.Modules.Workflow.Infrastructure;
using Cheetah.Modules.Workflow.Infrastructure.Extensions;
using Cheetah.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Workflow.Default.Modules;

/// <summary>
/// Готовый к работе модуль Workflow «из коробки»: конкретное правило <see cref="AutomationRule"/>,
/// <see cref="WorkflowDbContext"/> с миграцией, generic-инфраструктура и приложение, встроенное действие
/// <c>Log</c> и подписка движка на firehose. Приложение подключает этот модуль — и автоматизация работает.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmWorkflowModule),
    typeof(CheetahWorkflowDomainModule),
    typeof(CheetahWorkflowContractsModule),
    typeof(CheetahWorkflowApplicationModule),
    typeof(CheetahWorkflowInfrastructureModule),
    typeof(CrmAspNetCoreModule))]
public partial class CheetahWorkflowDefaultModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services); // [Export]: LogAction, RuleFactory, RuleProjector

        context.Services.AddWorkflowInfrastructure<WorkflowDbContext, AutomationRule>();
        context.Services.AddWorkflowApplication<AutomationRule, CreateAutomationRuleRequest,
            AutomationRuleDto, RuleFactory, RuleProjector>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // Админка/тест/журнал.
        new AutomationRuleEndpoints().Map(context.GetRouteBuilder());

        // Подписка движка на firehose-поток. Публикацию конвертов обеспечивает WorkflowEventForwarder
        // (host-level декоратор IEventBus, см. AddWorkflowFirehose).
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<WorkflowEventEnvelope, WorkflowEnvelopeHandler>();
    }
}
