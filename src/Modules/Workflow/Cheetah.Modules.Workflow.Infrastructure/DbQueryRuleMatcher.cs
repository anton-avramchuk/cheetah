using Cheetah.Modules.Workflow.Application;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Domain.Repositories;
using Cheetah.Modules.Workflow.Domain.Specifications;

namespace Cheetah.Modules.Workflow.Infrastructure;

/// <summary>
/// Реализация матчинга через БД: активные правила с Event-триггером на данное имя события, загруженные
/// вместе с детьми. Кэш-индекс <c>eventName → ruleIds</c> (ICacheService) — follow-up для 10k RPS.
/// </summary>
public sealed class DbQueryRuleMatcher<TRule> : IRuleMatcher<TRule> where TRule : AutomationRuleBase
{
    private readonly IAutomationRuleRepository<TRule> _repository;

    public DbQueryRuleMatcher(IAutomationRuleRepository<TRule> repository) => _repository = repository;

    public async ValueTask<IReadOnlyList<TRule>> MatchAsync(string eventName, Guid? tenantId, CancellationToken ct)
        => await _repository.ListAsync(
            new ActiveRulesByEventSpecification<TRule>(eventName, tenantId), includeChildren: true, ct);
}
