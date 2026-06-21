using Cheetah.Core.Events;

namespace Cheetah.Modules.CustomFields.DomainEvents;

// Чистые контракты интеграционных событий модуля кастомных полей. Публикуются через шину
// после SaveChangesAsync. DomainEvents не зависит от Shared/Domain — поля передаются примитивами.

/// <summary>Создано определение кастомного поля.</summary>
public record CustomFieldDefinitionCreatedIntegrationEvent(
    Guid Id, Guid? TenantId, string EntityType, string Key) : EventBase;

/// <summary>Изменены метаданные определения (→ инвалидация кэша определений типа).</summary>
public record CustomFieldDefinitionChangedIntegrationEvent(
    Guid Id, Guid? TenantId, string EntityType, string Key) : EventBase;

/// <summary>Определение деактивировано (soft-delete) (→ инвалидация кэша).</summary>
public record CustomFieldDefinitionDeactivatedIntegrationEvent(
    Guid Id, Guid? TenantId, string EntityType, string Key) : EventBase;

/// <summary>Изменён набор значений кастомных полей сущности.</summary>
public record CustomFieldValuesChangedIntegrationEvent(
    Guid? TenantId, string EntityType, string EntityId) : EventBase;
