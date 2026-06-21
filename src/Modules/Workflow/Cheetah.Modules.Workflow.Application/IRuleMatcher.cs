using Cheetah.Modules.Workflow.Domain.Entities;

namespace Cheetah.Modules.Workflow.Application;

/// <summary>
/// Подбирает активные правила, реагирующие на данное имя события (горячий путь движка). Реализация в
/// Infrastructure держит индекс <c>eventName → ruleIds</c> в кэше и инвалидирует его по событиям
/// изменения правил.
/// </summary>
public interface IRuleMatcher<TRule> where TRule : AutomationRuleBase
{
    ValueTask<IReadOnlyList<TRule>> MatchAsync(string eventName, Guid? tenantId, CancellationToken ct);
}
