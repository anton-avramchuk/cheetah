using Cheetah.Modules.Notes.Domain.Entities;

namespace Cheetah.Modules.Notes.Domain.Abstractions;

/// <summary>
/// Порт постраничного чтения заметок. Нужен потому, что списки — горячий путь: страницу обязан
/// отбирать и сортировать сервер БД, а не память процесса (<c>IRepository.GetAllAsync</c> материализует
/// весь результат спецификации). Реализация — в Infrastructure поверх EF (без трекинга); фильтрация
/// внутри неё идёт через те же спецификации домена.
/// </summary>
public interface INoteReader<TNote>
    where TNote : NoteBase
{
    /// <summary>
    /// Страница активных заметок сущности: закреплённые первыми, затем новые сверху.
    /// Границы <paramref name="skip"/>/<paramref name="take"/> вызывающий нормализует сам.
    /// </summary>
    ValueTask<IReadOnlyList<TNote>> GetPageByEntityAsync(
        string entityType, Guid entityId, bool pinnedOnly, int skip, int take, CancellationToken ct = default);

    /// <summary>Страница ветки треда — ответы на заметку, старые сверху.</summary>
    ValueTask<IReadOnlyList<TNote>> GetRepliesPageAsync(
        Guid parentNoteId, int skip, int take, CancellationToken ct = default);
}
