using Cheetah.Modules.Workflow.Api.Endpoints;
using Cheetah.Modules.Workflow.Default.Contracts;

namespace Cheetah.Modules.Workflow.Default.Endpoints;

/// <summary>Конкретные эндпоинты «из коробки» для правил <see cref="AutomationRuleDto"/>.</summary>
public sealed class AutomationRuleEndpoints : AutomationRuleEndpointsBase<CreateAutomationRuleRequest, AutomationRuleDto>;
