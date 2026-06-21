using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Modules.Workflow.Domain.Entities;

namespace Cheetah.Modules.Workflow.Application;

/// <summary>Создаёт конкретный агрегат правила из запроса (замена <c>new</c> абстрактной сущности).</summary>
public interface IAutomationRuleFactory<TRule, in TCreateRequest>
    where TRule : AutomationRuleBase
    where TCreateRequest : CreateAutomationRuleRequestBase
{
    TRule Create(TCreateRequest request);
}

/// <summary>Проецирует агрегат правила в его ViewModel.</summary>
public interface IAutomationRuleProjector<in TRule, out TDto>
    where TRule : AutomationRuleBase
    where TDto : AutomationRuleDtoBase
{
    TDto ToDto(TRule rule);
}
