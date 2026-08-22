using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Notes.Application.Abstractions;
using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Domain.Abstractions;
using Cheetah.Modules.Notes.Domain.Entities;
using Cheetah.Modules.Notes.Shared;

namespace Cheetah.Modules.Notes.Application.Notes;

/// <summary>Заметка по идентификатору (удалённые не отдаются — скрыты query-фильтром).</summary>
public sealed record GetNoteByIdQuery<TDto>(Guid Id) : IQuery<TDto?>
    where TDto : NoteDtoBase;

public class GetNoteByIdQueryHandler<TNote, TDto> : IQueryHandler<GetNoteByIdQuery<TDto>, TDto?>
    where TNote : NoteBase
    where TDto : NoteDtoBase
{
    private readonly IRepository<TNote, Guid> _repository;
    private readonly INoteProjector<TNote, TDto> _projector;

    public GetNoteByIdQueryHandler(IRepository<TNote, Guid> repository, INoteProjector<TNote, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetNoteByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var note = await _repository.GetByIdAsync(query.Id, ct);
        return note is null ? null : _projector.ToDto(note);
    }
}

/// <summary>
/// Страница активных заметок сущности (закреплённые — первыми, затем новые сверху). Отбор,
/// сортировка и срез выполняются в БД через <see cref="INoteReader{TNote}"/>.
/// </summary>
public sealed record GetNotesByEntityQuery<TDto>(
    string EntityType, Guid EntityId, bool PinnedOnly = false, int Skip = 0,
    int Take = NotesConstants.DefaultPageSize) : IQuery<IReadOnlyList<TDto>>
    where TDto : NoteDtoBase;

public class GetNotesByEntityQueryHandler<TNote, TDto> : IQueryHandler<GetNotesByEntityQuery<TDto>, IReadOnlyList<TDto>>
    where TNote : NoteBase
    where TDto : NoteDtoBase
{
    private readonly INoteReader<TNote> _reader;
    private readonly INoteProjector<TNote, TDto> _projector;

    public GetNotesByEntityQueryHandler(INoteReader<TNote> reader, INoteProjector<TNote, TDto> projector)
    {
        _reader = reader;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(
        GetNotesByEntityQuery<TDto> query, CancellationToken ct = default)
    {
        var notes = await _reader.GetPageByEntityAsync(
            NotePaging.NormalizeEntityType(query.EntityType), query.EntityId, query.PinnedOnly,
            NotePaging.NormalizeSkip(query.Skip), NotePaging.NormalizeTake(query.Take), ct);

        return notes.Select(_projector.ToDto).ToArray();
    }
}

/// <summary>Страница ветки треда — ответы на заметку (старые сверху, как в переписке).</summary>
public sealed record GetNoteRepliesQuery<TDto>(
    Guid ParentNoteId, int Skip = 0, int Take = NotesConstants.DefaultPageSize) : IQuery<IReadOnlyList<TDto>>
    where TDto : NoteDtoBase;

public class GetNoteRepliesQueryHandler<TNote, TDto> : IQueryHandler<GetNoteRepliesQuery<TDto>, IReadOnlyList<TDto>>
    where TNote : NoteBase
    where TDto : NoteDtoBase
{
    private readonly INoteReader<TNote> _reader;
    private readonly INoteProjector<TNote, TDto> _projector;

    public GetNoteRepliesQueryHandler(INoteReader<TNote> reader, INoteProjector<TNote, TDto> projector)
    {
        _reader = reader;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(
        GetNoteRepliesQuery<TDto> query, CancellationToken ct = default)
    {
        var notes = await _reader.GetRepliesPageAsync(
            query.ParentNoteId,
            NotePaging.NormalizeSkip(query.Skip), NotePaging.NormalizeTake(query.Take), ct);

        return notes.Select(_projector.ToDto).ToArray();
    }
}

/// <summary>
/// Нормализация параметров чтения: клиент не должен уметь запросить всю таблицу, а ключ типа
/// сущности на чтении приводится так же, как на записи (<c>NoteBase</c> хранит его обрезанным).
/// </summary>
internal static class NotePaging
{
    public static string NormalizeEntityType(string entityType) => entityType?.Trim() ?? string.Empty;

    public static int NormalizeSkip(int skip) => Math.Max(0, skip);

    public static int NormalizeTake(int take) => Math.Clamp(
        take <= 0 ? NotesConstants.DefaultPageSize : take, 1, NotesConstants.MaxPageSize);
}
