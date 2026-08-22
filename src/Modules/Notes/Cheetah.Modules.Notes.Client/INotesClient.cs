using Cheetah.Modules.Notes.Contracts;

namespace Cheetah.Modules.Notes.Client;

/// <summary>
/// HTTP-клиент к Notes.Api для server-to-server интеграции. Generic над закрытыми типами наследника:
/// приложение подставляет свои <c>CreateNoteRequest</c>/<c>UpdateNoteRequest</c>/<c>NoteDto</c>,
/// поэтому доп. поля расширенной заметки ходят по проводу без правок модуля.
/// </summary>
public interface INotesClient<in TCreateRequest, in TUpdateRequest, TDto>
    where TCreateRequest : CreateNoteRequestBase
    where TUpdateRequest : UpdateNoteRequestBase
    where TDto : NoteDtoBase
{
    /// <summary>Создать заметку. Возвращает Id.</summary>
    ValueTask<Guid> CreateAsync(TCreateRequest request, CancellationToken ct = default);

    /// <summary>Заметка по Id; null, если не найдена или удалена.</summary>
    ValueTask<TDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Заметки сущности: закреплённые первыми, затем новые сверху.</summary>
    ValueTask<IReadOnlyList<TDto>> GetByEntityAsync(
        string entityType, Guid entityId, bool pinnedOnly = false, int skip = 0, int take = 0,
        CancellationToken ct = default);

    /// <summary>Ветка треда — ответы на заметку, постранично.</summary>
    ValueTask<IReadOnlyList<TDto>> GetRepliesAsync(
        Guid id, int skip = 0, int take = 0, CancellationToken ct = default);

    /// <summary>Отредактировать тело и упоминания.</summary>
    ValueTask UpdateAsync(Guid id, TUpdateRequest request, CancellationToken ct = default);

    ValueTask PinAsync(Guid id, CancellationToken ct = default);

    ValueTask UnpinAsync(Guid id, CancellationToken ct = default);

    /// <summary>Удалить заметку (soft-delete).</summary>
    ValueTask RemoveAsync(Guid id, CancellationToken ct = default);
}
