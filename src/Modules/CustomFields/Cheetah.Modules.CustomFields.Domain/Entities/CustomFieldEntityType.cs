using Cheetah.Core.Domain;
using Cheetah.Modules.CustomFields.Shared;

namespace Cheetah.Modules.CustomFields.Domain.Entities;

/// <summary>
/// Зарегистрированный тип сущности, который можно расширять кастомными полями (каталог).
/// Глобальный контракт платформы (не тенант-данные), наполняется через registry/sync при старте сервисов.
/// </summary>
public sealed class CustomFieldEntityType : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Key { get; private set; } = null!;            // "crm.deal"
    public string DisplayName { get; private set; } = null!;
    public string OwnerService { get; private set; } = null!;
    public CustomFieldEntityIdType IdType { get; private set; }
    public bool IsActive { get; private set; } = true;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private CustomFieldEntityType() { } // EF

    public static CustomFieldEntityType Create(string key, string displayName, string ownerService,
        CustomFieldEntityIdType idType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerService);
        return new CustomFieldEntityType
        {
            Id = Guid.NewGuid(),
            Key = key.Trim(),
            DisplayName = displayName,
            OwnerService = ownerService,
            IdType = idType,
            IsActive = true
        };
    }

    /// <summary>Идемпотентное обновление метаданных типа (registry/sync при рестарте сервиса).</summary>
    public void Update(string displayName, string ownerService, CustomFieldEntityIdType idType)
    {
        DisplayName = displayName;
        OwnerService = ownerService;
        IdType = idType;
        IsActive = true;
    }

    public void Deprecate() => IsActive = false;
}
