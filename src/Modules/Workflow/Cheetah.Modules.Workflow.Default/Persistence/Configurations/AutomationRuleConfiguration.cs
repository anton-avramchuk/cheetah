using Cheetah.Modules.Workflow.Default.Entities;
using Cheetah.Modules.Workflow.Infrastructure.Persistence.Configurations;

namespace Cheetah.Modules.Workflow.Default.Persistence.Configurations;

/// <summary>Конкретная конфигурация правила «из коробки» (без доп. колонок).</summary>
public sealed class AutomationRuleConfiguration : AutomationRuleConfigurationBase<AutomationRule>;
