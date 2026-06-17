using Cheetah.Core.Events;

namespace Cheetah.Modules.NotesTimeline.DomainEvents;

/// <summary>Заметка создана и привязана к сущности <c>(EntityType, EntityId)</c>.</summary>
public record NoteCreatedIntegrationEvent(
    Guid NoteId, string EntityType, Guid EntityId, Guid AuthorId) : EventBase;

/// <summary>Тело заметки отредактировано.</summary>
public record NoteUpdatedIntegrationEvent(Guid NoteId) : EventBase;

/// <summary>Заметка удалена (soft-delete).</summary>
public record NoteRemovedIntegrationEvent(Guid NoteId, string EntityType, Guid EntityId) : EventBase;

/// <summary>
/// Пользователь упомянут в заметке (<c>@user</c>) — доставку уведомления делает Notification.
/// Публикуется по одному событию на каждое новое упоминание.
/// </summary>
public record UserMentionedIntegrationEvent(
    Guid NoteId, string EntityType, Guid EntityId, Guid MentionedUserId, Guid ByUserId) : EventBase;
