using Cheetah.Core.Domain;

namespace Cheetah.Modules.Tags.Domain.Entities;

/// <summary>
/// Запись каталога применимых типов: какой тип сущности (по ключу <c>"{service}.{entity}"</c>)
/// может быть тэгирован и с какими ограничениями. Источник истины — сервисы-владельцы,
/// которые регистрируют свои типы при старте (идемпотентный upsert).
/// </summary>
public class TaggableEntityType : Entity<string>, ICreateAtEntity, IUpdatedAtEntity
{
    public string DisplayName { get; private set; } = "";
    public string OwnerService { get; private set; } = "";

    /// <summary>Максимум тэгов на одну сущность этого типа. null = без лимита.</summary>
    public int? MaxTagsPerEntity { get; private set; }

    /// <summary>Можно ли создавать тэги «на лету» при назначении.</summary>
    public bool AllowAdHocTags { get; private set; }

    /// <summary>Если непусто — назначать можно только тэги с <see cref="Tag.Group"/> из этого набора.</summary>
    public List<string> AllowedGroups { get; private set; } = new();

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private TaggableEntityType() { } // EF

    public static TaggableEntityType Create(
        string key,
        string displayName,
        string ownerService,
        int? maxTagsPerEntity,
        bool allowAdHocTags,
        IEnumerable<string>? allowedGroups)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Taggable entity type key cannot be empty", nameof(key));

        var entity = new TaggableEntityType { Id = key };
        entity.Apply(displayName, ownerService, maxTagsPerEntity, allowAdHocTags, allowedGroups);
        return entity;
    }

    public void Update(
        string displayName,
        string ownerService,
        int? maxTagsPerEntity,
        bool allowAdHocTags,
        IEnumerable<string>? allowedGroups)
        => Apply(displayName, ownerService, maxTagsPerEntity, allowAdHocTags, allowedGroups);

    private void Apply(
        string displayName,
        string ownerService,
        int? maxTagsPerEntity,
        bool allowAdHocTags,
        IEnumerable<string>? allowedGroups)
    {
        DisplayName = displayName ?? "";
        OwnerService = ownerService ?? "";
        MaxTagsPerEntity = maxTagsPerEntity;
        AllowAdHocTags = allowAdHocTags;
        AllowedGroups = allowedGroups?.Where(g => !string.IsNullOrWhiteSpace(g)).Distinct().ToList() ?? new();
    }

    /// <summary>Допустим ли тэг с указанной группой для этого типа сущности.</summary>
    public bool IsGroupAllowed(string? group)
        => AllowedGroups.Count == 0 || (group is not null && AllowedGroups.Contains(group));
}
