using Cheetah.Core.Domain;

namespace Cheetah.Modules.Tags.Domain.Entities;

/// <summary>
/// Связь «тэг ↔ сущность другого сервиса». Сущность адресуется парой
/// (<see cref="EntityType"/> — строковый ключ типа, <see cref="EntityId"/> — Guid).
/// </summary>
public class TagAssignment : Entity<Guid>, ICreateAtEntity
{
    public Guid TagId { get; private set; }
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }

    /// <summary>Id пользователя-инициатора (FK на локальную реплику <see cref="User"/>). Опционально.</summary>
    public Guid? AssignedBy { get; private set; }

    /// <summary>Навигация на пользователя-инициатора из реплики. null, если назначение системное или пользователь не в реплике.</summary>
    public User? AssignedByUser { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    private TagAssignment() { } // EF

    public static TagAssignment Create(Guid tagId, string entityType, Guid entityId, Guid? assignedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        if (entityId == Guid.Empty)
            throw new ArgumentException("EntityId cannot be empty", nameof(entityId));

        return new TagAssignment
        {
            Id = Guid.NewGuid(),
            TagId = tagId,
            EntityType = entityType,
            EntityId = entityId,
            AssignedBy = assignedBy
        };
    }
}
