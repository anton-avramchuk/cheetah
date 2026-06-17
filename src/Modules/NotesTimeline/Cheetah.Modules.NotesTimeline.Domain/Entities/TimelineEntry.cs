using Cheetah.Core.Domain;

namespace Cheetah.Modules.NotesTimeline.Domain.Entities;

/// <summary>
/// Строка ленты хронологии сущности — read-model, материализуемая из интеграционных событий других
/// модулей и заметок. Append-only и неизменяема: правки/удаления родительской сущности добавляют
/// новые строки (<c>*-updated</c>/<c>*-removed</c>), а не редактируют существующие. Конкретная
/// (не расширяемая полями) сущность: гибкость хронологии — в произвольной нагрузке <see cref="Payload"/>.
/// </summary>
public sealed class TimelineEntry : Entity<Guid>
{
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public string Kind { get; private set; } = null!;
    public string Title { get; private set; } = null!;

    /// <summary>Произвольная нагрузка вида (jsonb): детали факта для рендера на фронте.</summary>
    public string? Payload { get; private set; }

    /// <summary>Кто инициировал факт (Identity); null — системное действие.</summary>
    public Guid? ActorId { get; private set; }

    /// <summary>Время факта (из события-источника), не время вставки строки.</summary>
    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>Идентификатор события-источника — для идемпотентности материализации (анти-дубль).</summary>
    public string? SourceEventId { get; private set; }

    private TimelineEntry() { }

    public static TimelineEntry Create(
        string entityType, Guid entityId, string kind, string title, DateTimeOffset occurredAt,
        Guid? actorId = null, string? payload = null, string? sourceEventId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new TimelineEntry
        {
            Id = Guid.NewGuid(),
            EntityType = entityType.Trim(),
            EntityId = entityId,
            Kind = kind.Trim(),
            Title = title.Trim(),
            OccurredAt = occurredAt,
            ActorId = actorId,
            Payload = payload,
            SourceEventId = sourceEventId
        };
    }
}
