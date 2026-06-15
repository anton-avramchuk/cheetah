using Cheetah.Core.Events;

namespace Cheetah.Modules.Activities.DomainEvents;

/// <summary>Активность создана и привязана к сущности <c>(EntityType, EntityId)</c>.</summary>
public record ActivityCreatedIntegrationEvent(
    Guid ActivityId, string EntityType, Guid EntityId, Guid AssigneeId, DateTimeOffset? DueAt) : EventBase;

/// <summary>Активность завершена.</summary>
public record ActivityCompletedIntegrationEvent(Guid ActivityId, Guid CompletedBy) : EventBase;

/// <summary>Активность отменена.</summary>
public record ActivityCanceledIntegrationEvent(Guid ActivityId) : EventBase;

/// <summary>Исполнитель активности изменён.</summary>
public record ActivityReassignedIntegrationEvent(Guid ActivityId, Guid OldAssigneeId, Guid NewAssigneeId) : EventBase;

/// <summary>Подошёл срок активности (по напоминанию) — доставку делает Notification.</summary>
public record ActivityDueIntegrationEvent(Guid ActivityId, Guid AssigneeId) : EventBase;

/// <summary>Срок активности истёк (просрочка).</summary>
public record ActivityOverdueIntegrationEvent(Guid ActivityId, Guid AssigneeId) : EventBase;
