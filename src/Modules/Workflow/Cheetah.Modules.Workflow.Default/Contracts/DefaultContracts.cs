using Cheetah.Modules.Workflow.Contracts;

namespace Cheetah.Modules.Workflow.Default.Contracts;

/// <summary>Конкретный запрос создания правила «из коробки».</summary>
public sealed record CreateAutomationRuleRequest : CreateAutomationRuleRequestBase;

/// <summary>Конкретная ViewModel правила «из коробки».</summary>
public sealed record AutomationRuleDto : AutomationRuleDtoBase;
