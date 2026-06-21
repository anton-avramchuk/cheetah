using Cheetah.Core.Domain;
using Cheetah.Modules.Workflow.DomainEvents;
using Cheetah.Modules.Workflow.Shared;

namespace Cheetah.Modules.Workflow.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат правила автоматизации. Шаблонный модуль не инстанцирует его сам —
/// наследник объявляет <c>sealed class AutomationRule : AutomationRuleBase</c> со своей фабрикой (через
/// <see cref="InitializeCore"/>) и доп. полями (например, <c>OwnerTeam</c>, <c>JiraTicket</c>).
/// Структурная точка расширяемости; поведенческая — plugin-действия <c>IWorkflowAction</c>.
/// </summary>
public abstract class AutomationRuleBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<TriggerBinding> _triggers = new();
    private readonly List<RuleAction> _actions = new();

    public string Name { get; protected set; } = null!;
    public string OwnerService { get; protected set; } = null!;
    public string? Description { get; protected set; }

    /// <summary>JsonLogic-выражение над payload триггера; <c>null</c> = условие всегда истинно.</summary>
    public string? ConditionExpression { get; protected set; }

    public bool IsActive { get; protected set; }
    public Guid? TenantId { get; protected set; }

    public IReadOnlyList<TriggerBinding> Triggers => _triggers;
    public IReadOnlyList<RuleAction> Actions => _actions;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected AutomationRuleBase() { } // EF + наследник

    /// <summary>Заводит инварианты нового правила и событие создания. Вызывается фабрикой наследника.
    /// По умолчанию правило выключено.</summary>
    protected void InitializeCore(Guid id, string name, string ownerService, string? description, Guid? tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerService);

        Id = id;
        Name = name.Trim();
        OwnerService = ownerService.Trim();
        Description = description;
        TenantId = tenantId;
        IsActive = false;

        AddDomainEvent(new AutomationRuleCreatedIntegrationEvent(Id, Name, OwnerService));
    }

    public virtual void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public virtual void Enable()
    {
        if (IsActive) return;
        EnsureExecutable();
        IsActive = true;
        AddDomainEvent(new AutomationRuleEnabledIntegrationEvent(Id));
    }

    public virtual void Disable()
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(new AutomationRuleDisabledIntegrationEvent(Id));
    }

    public virtual void SetCondition(string? jsonLogic) => ConditionExpression = jsonLogic;

    public virtual void SetTriggers(IEnumerable<TriggerBinding> bindings)
    {
        _triggers.Clear();
        _triggers.AddRange(bindings);
        AddDomainEvent(new AutomationRuleChangedIntegrationEvent(Id, TriggerKeys()));
    }

    public virtual void SetActions(IEnumerable<RuleAction> actions)
    {
        _actions.Clear();
        _actions.AddRange(actions.OrderBy(a => a.Order));
    }

    /// <summary>Имена событий-триггеров — для построения кэш-индекса event→rules и его инвалидации.</summary>
    public IReadOnlyList<string> EventTriggerKeys()
        => _triggers.Where(t => t.TriggerType == TriggerType.Event).Select(t => t.TriggerKey).ToArray();

    private IReadOnlyList<string> TriggerKeys() => _triggers.Select(t => t.TriggerKey).ToArray();

    private void EnsureExecutable()
    {
        if (_triggers.Count == 0)
            throw new InvalidOperationException("Rule has no triggers.");
        if (_actions.Count == 0)
            throw new InvalidOperationException("Rule has no actions.");
    }
}
