using Cheetah.Core.Domain;
using Cheetah.Modules.Notes.DomainEvents;
using Cheetah.Modules.Notes.Shared;

namespace Cheetah.Modules.Notes.Domain.Entities;

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
    protected readonly List<Guid> _mentions = new();
    protected readonly List<Guid> _attachmentFileIds = new();

    /// <summary>Полиморфная привязка к произвольной сущности CRM (deal/customer/contact/lead/…).</summary>
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }

    public Guid AuthorId { get; private set; }
    public string Body { get; private set; } = null!;

    public IReadOnlyList<Guid> Mentions => _mentions;
    public IReadOnlyList<Guid> AttachmentFileIds => _attachmentFileIds;

    /// <summary>Родительская заметка треда (ответ на заметку).</summary>
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
    /// Длины проверяются здесь же — иначе ошибка всплыла бы уже как <c>DbUpdateException</c> (500)
    /// на границе varchar-колонок.
    /// </summary>
    protected void InitializeCore(
        Guid id, string entityType, Guid entityId, Guid authorId, string body,
        IEnumerable<Guid>? mentions = null, IEnumerable<Guid>? attachmentFileIds = null,
        Guid? parentNoteId = null)
    {
        Id = id;
        EntityType = NormalizeEntityType(entityType);
        EntityId = entityId;
        AuthorId = authorId;
        Body = NormalizeBody(body);
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
    /// Редактирует тело и (опционально) набор упоминаний. Доп. поля наследника обновляет он сам
    /// (переопределив метод). Событие упоминания публикуется только для НОВЫХ упомянутых.
    /// <para>
    /// <paramref name="mentions"/> = <c>null</c> означает «не трогать упоминания»: правка одного
    /// лишь текста не должна молча стирать существующие. Чтобы очистить набор, передайте пустую
    /// коллекцию.
    /// </para>
    /// </summary>
    public virtual void Edit(string body, IEnumerable<Guid>? mentions = null)
    {
        Body = NormalizeBody(body);

        AddDomainEvent(new NoteUpdatedIntegrationEvent(Id));

        if (mentions is null)
            return;

        var before = _mentions.ToHashSet();
        _mentions.Clear();
        _mentions.AddRange(mentions.Distinct());

        foreach (var mentioned in _mentions.Where(m => !before.Contains(m)))
            AddDomainEvent(new UserMentionedIntegrationEvent(Id, EntityType, EntityId, mentioned, AuthorId));
    }

    /// <summary>
    /// Заменяет набор вложений (id файлов внешнего хранилища). <c>null</c> — «не трогать», как и в
    /// <see cref="Edit"/>; пустая коллекция отвязывает все файлы.
    /// </summary>
    public void ChangeAttachments(IEnumerable<Guid>? attachmentFileIds)
    {
        if (attachmentFileIds is null)
            return;

        _attachmentFileIds.Clear();
        _attachmentFileIds.AddRange(attachmentFileIds.Distinct());
    }

    public void Pin()
    {
        if (PinnedAt is not null)
            return;

        PinnedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new NotePinnedIntegrationEvent(Id, EntityType, EntityId));
    }

    public void Unpin()
    {
        if (PinnedAt is null)
            return;

        PinnedAt = null;
        AddDomainEvent(new NoteUnpinnedIntegrationEvent(Id, EntityType, EntityId));
    }

    /// <summary>Soft-delete: помечает заметку удалённой и публикует событие (один раз).</summary>
    public void Remove()
    {
        if (RemovedAt is not null)
            return;

        RemovedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new NoteRemovedIntegrationEvent(Id, EntityType, EntityId));
    }

    public void SetAttributes(string? attributes) => Attributes = attributes;

    private static string NormalizeEntityType(string entityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        var value = entityType.Trim();
        if (value.Length > NotesConstants.MaxEntityTypeLength)
            throw new ArgumentException(
                $"EntityType exceeds {NotesConstants.MaxEntityTypeLength} characters", nameof(entityType));

        return value;
    }

    private static string NormalizeBody(string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        var value = body.Trim();
        if (value.Length > NotesConstants.MaxBodyLength)
            throw new ArgumentException(
                $"Body exceeds {NotesConstants.MaxBodyLength} characters", nameof(body));

        return value;
    }
}
