using Cheetah.Core.Domain;
using Cheetah.Modules.NotesTimeline.DomainEvents;

namespace Cheetah.Modules.NotesTimeline.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат заметки. Шаблонный модуль не инстанцирует его сам — наследник
/// объявляет конкретный <c>sealed class Note : NoteBase</c> со своей фабрикой (через
/// <see cref="InitializeCore"/>) и доп. полями. Это и есть точка расширяемости сущности.
/// <para>
/// Привязка к произвольной сущности CRM — полиморфная <c>(EntityType, EntityId)</c>. Упоминания и
/// вложения — коллекции в агрегате (упоминания публикуют <see cref="UserMentionedIntegrationEvent"/>).
/// </para>
/// </summary>
public abstract class NoteBase : AggregateRoot<Guid>,
    ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    private readonly List<Guid> _mentions = new();
    private readonly List<Guid> _attachmentFileIds = new();

    /// <summary>Полиморфная привязка к произвольной сущности CRM (deal/customer/contact/lead/…).</summary>
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }

    public Guid AuthorId { get; private set; }
    public string Body { get; private set; } = null!;

    public IReadOnlyList<Guid> Mentions => _mentions;
    public IReadOnlyList<Guid> AttachmentFileIds => _attachmentFileIds;

    /// <summary>Родительская заметка (зарезервировано под треды/ответы; на MVP не используется).</summary>
    public Guid? ParentNoteId { get; private set; }

    public DateTimeOffset? PinnedAt { get; private set; }

    /// <summary>«Быстрый» карман расширения без миграций (jsonb). Полноценно — модуль Custom Fields.</summary>
    public string? Attributes { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }

    protected NoteBase() { } // EF + наследник

    /// <summary>
    /// Заводит инварианты новой заметки, событие создания и события по каждому упоминанию.
    /// Вызывается фабрикой наследника (замена <c>new</c> абстрактной сущности).
    /// </summary>
    protected void InitializeCore(
        Guid id, string entityType, Guid entityId, Guid authorId, string body,
        IEnumerable<Guid>? mentions = null, IEnumerable<Guid>? attachmentFileIds = null,
        Guid? parentNoteId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        Id = id;
        EntityType = entityType.Trim();
        EntityId = entityId;
        AuthorId = authorId;
        Body = body.Trim();
        ParentNoteId = parentNoteId;

        if (mentions is not null)
            _mentions.AddRange(mentions.Distinct());
        if (attachmentFileIds is not null)
            _attachmentFileIds.AddRange(attachmentFileIds.Distinct());

        AddDomainEvent(new NoteCreatedIntegrationEvent(Id, EntityType, EntityId, AuthorId));
        foreach (var mentioned in _mentions)
            AddDomainEvent(new UserMentionedIntegrationEvent(Id, EntityType, EntityId, mentioned, AuthorId));
    }

    /// <summary>
    /// Редактирует тело и набор упоминаний. Доп. поля наследника обновляет он сам (переопределив метод).
    /// Событие упоминания публикуется только для НОВЫХ упомянутых пользователей.
    /// </summary>
    public virtual void Edit(string body, IEnumerable<Guid>? mentions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        Body = body.Trim();

        var before = _mentions.ToHashSet();
        _mentions.Clear();
        if (mentions is not null)
            _mentions.AddRange(mentions.Distinct());

        AddDomainEvent(new NoteUpdatedIntegrationEvent(Id));
        foreach (var mentioned in _mentions.Where(m => !before.Contains(m)))
            AddDomainEvent(new UserMentionedIntegrationEvent(Id, EntityType, EntityId, mentioned, AuthorId));
    }

    public void Pin()
    {
        if (PinnedAt is null)
            PinnedAt = DateTimeOffset.UtcNow;
    }

    public void Unpin() => PinnedAt = null;

    /// <summary>Soft-delete: помечает заметку удалённой и публикует событие (один раз).</summary>
    public void Remove()
    {
        if (RemovedAt is not null)
            return;

        RemovedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new NoteRemovedIntegrationEvent(Id, EntityType, EntityId));
    }

    public void SetAttributes(string? attributes) => Attributes = attributes;
}
