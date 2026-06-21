using Cheetah.Core.Events;

namespace Cheetah.Modules.Workflow.DomainEvents;

/// <summary>Создано новое правило автоматизации.</summary>
public record AutomationRuleCreatedIntegrationEvent(Guid RuleId, string Name, string OwnerService) : EventBase;

/// <summary>Правило включено — потребители инвалидируют кэш-индекс event→rules.</summary>
public record AutomationRuleEnabledIntegrationEvent(Guid RuleId) : EventBase;

/// <summary>Правило выключено — потребители инвалидируют кэш-индекс event→rules.</summary>
public record AutomationRuleDisabledIntegrationEvent(Guid RuleId) : EventBase;

/// <summary>Изменён состав триггеров/условий правила — потребители инвалидируют кэш-индекс.</summary>
public record AutomationRuleChangedIntegrationEvent(Guid RuleId, IReadOnlyList<string> TriggerKeys) : EventBase;

/// <summary>Срабатывание правила завершено (Status — строковое значение RunStatus).</summary>
public record AutomationRunCompletedIntegrationEvent(Guid RunId, Guid RuleId, string Status) : EventBase;

/// <summary>Срабатывание правила завершилось ошибкой — может уведомить владельца (Notification).</summary>
public record AutomationRunFailedIntegrationEvent(Guid RunId, Guid RuleId, string Error) : EventBase;
