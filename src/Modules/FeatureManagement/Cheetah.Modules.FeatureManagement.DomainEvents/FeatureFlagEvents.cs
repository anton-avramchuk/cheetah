using Cheetah.Core.Events;

namespace Cheetah.Modules.FeatureManagement.DomainEvents;

/// <summary>Флаг зарегистрирован в каталоге (создан).</summary>
public record FeatureFlagCreatedIntegrationEvent(string Key, string OwnerService) : EventBase;

/// <summary>
/// Изменился таргетинг/состав/override флага — потребители сбрасывают локальный кэш/реплику ключа.
/// <paramref name="TenantId"/> непуст, если изменение касается конкретного тенанта.
/// </summary>
public record FeatureFlagChangedIntegrationEvent(string Key, Guid? TenantId) : EventBase;

/// <summary>Переключён kill-switch флага (enable/disable).</summary>
public record FeatureFlagToggledIntegrationEvent(string Key, bool Enabled) : EventBase;
