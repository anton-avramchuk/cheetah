using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Shared;

namespace Cheetah.Modules.Workflow.Domain.Specifications;

/// <summary>Правило по идентификатору.</summary>
public sealed class RuleByIdSpecification<TRule>(Guid id) : Specification<TRule>
    where TRule : AutomationRuleBase
{
    public override Expression<Func<TRule, bool>> ToExpression() => r => r.Id == id;
}

/// <summary>Горячий путь матчинга: активные правила с Event-триггером на данное имя события (с учётом тенанта).</summary>
public sealed class ActiveRulesByEventSpecification<TRule>(string eventName, Guid? tenantId) : Specification<TRule>
    where TRule : AutomationRuleBase
{
    public override Expression<Func<TRule, bool>> ToExpression()
        => r => r.IsActive
             && (r.TenantId == null || r.TenantId == tenantId)
             && r.Triggers.Any(t => t.TriggerType == TriggerType.Event && t.TriggerKey == eventName);
}

/// <summary>Правила по нескольким идентификаторам (загрузка по индексу из кэша).</summary>
public sealed class RulesByIdsSpecification<TRule>(IReadOnlyCollection<Guid> ids) : Specification<TRule>
    where TRule : AutomationRuleBase
{
    public override Expression<Func<TRule, bool>> ToExpression() => r => ids.Contains(r.Id);
}

/// <summary>Правила конкретного сервиса-владельца.</summary>
public sealed class RulesByOwnerServiceSpecification<TRule>(string ownerService) : Specification<TRule>
    where TRule : AutomationRuleBase
{
    public override Expression<Func<TRule, bool>> ToExpression() => r => r.OwnerService == ownerService;
}

/// <summary>Активные правила с расписанием (для cron-планировщика).</summary>
public sealed class ScheduledRulesSpecification<TRule>() : Specification<TRule>
    where TRule : AutomationRuleBase
{
    public override Expression<Func<TRule, bool>> ToExpression()
        => r => r.IsActive && r.Triggers.Any(t => t.TriggerType == TriggerType.Schedule);
}

/// <summary>Только включённые правила (фильтр списка).</summary>
public sealed class ActiveRulesSpecification<TRule>() : Specification<TRule>
    where TRule : AutomationRuleBase
{
    public override Expression<Func<TRule, bool>> ToExpression() => r => r.IsActive;
}

/// <summary>Прогоны конкретного правила.</summary>
public sealed class RunsByRuleSpecification(Guid ruleId) : Specification<AutomationRun>
{
    public override Expression<Func<AutomationRun, bool>> ToExpression() => r => r.RuleId == ruleId;
}

/// <summary>Прогоны в конкретном статусе.</summary>
public sealed class RunsByStatusSpecification(RunStatus status) : Specification<AutomationRun>
{
    public override Expression<Func<AutomationRun, bool>> ToExpression() => r => r.Status == status;
}

/// <summary>Запуск правила по конкретному событию — для дедупа (бэкстоп — уникальный индекс (RuleId, EventId)).</summary>
public sealed class RunByRuleAndEventSpecification(Guid ruleId, Guid eventId) : Specification<AutomationRun>
{
    public override Expression<Func<AutomationRun, bool>> ToExpression()
        => r => r.RuleId == ruleId && r.EventId == eventId;
}
