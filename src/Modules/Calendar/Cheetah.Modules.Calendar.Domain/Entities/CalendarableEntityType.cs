using Cheetah.Core.Domain;

namespace Cheetah.Modules.Calendar.Domain.Entities;

/// <summary>
/// Зарегистрированный тип сущности, к которой можно привязывать события (как реестр
/// taggable-типов в Tags). Calendar не знает заранее про «сделку» или «задачу» —
/// потребитель регистрирует свои типы при старте. Все привязки валидируются по этому каталогу.
/// </summary>
public class CalendarableEntityType : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    /// <summary>Канонический ключ типа, напр. "crm.deal".</summary>
    public string EntityType { get; private set; } = null!;

    public string DisplayName { get; private set; } = null!;
    public string? DefaultColor { get; private set; }
    public bool AllowMultiplePerEntity { get; private set; }
    public string? OwnerService { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private CalendarableEntityType() { } // EF

    public static CalendarableEntityType Create(
        string entityType, string displayName, string? defaultColor, bool allowMultiplePerEntity, string? ownerService)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        return new CalendarableEntityType
        {
            Id = Guid.NewGuid(),
            EntityType = entityType.Trim(),
            DisplayName = displayName.Trim(),
            DefaultColor = defaultColor,
            AllowMultiplePerEntity = allowMultiplePerEntity,
            OwnerService = ownerService
        };
    }

    public void Update(string displayName, string? defaultColor, bool allowMultiplePerEntity, string? ownerService)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        DisplayName = displayName.Trim();
        DefaultColor = defaultColor;
        AllowMultiplePerEntity = allowMultiplePerEntity;
        OwnerService = ownerService;
    }
}
