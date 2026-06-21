using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Workflow.Api.Endpoints;
using Cheetah.Modules.Workflow.Application;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Api;

/// <summary>
/// Абстрактный базовый Api-модуль Workflow: маппит админку/тест/журнал в
/// <see cref="OnApplicationInitialization"/>. Наследник закрывает generic своими типами:
/// <code>
/// public sealed class AppWorkflowApiModule
///     : CheetahWorkflowApiModuleBase&lt;AutomationRuleEndpoints, CreateAutomationRuleRequest, AutomationRuleDto&gt; { }
/// </code>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmWorkflowModule),
    typeof(CheetahWorkflowApplicationModule),
    typeof(CheetahWorkflowContractsModule))]
public abstract class CheetahWorkflowApiModuleBase<TEndpoints, TCreateRequest, TDto> : CrmModule
    where TEndpoints : AutomationRuleEndpointsBase<TCreateRequest, TDto>, new()
    where TCreateRequest : CreateAutomationRuleRequestBase
    where TDto : AutomationRuleDtoBase
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new TEndpoints().Map(context.GetRouteBuilder());
    }
}
