using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.NotesTimeline.Application.Abstractions;
using Cheetah.Modules.NotesTimeline.Contracts;
using Cheetah.Modules.NotesTimeline.Domain.Entities;
using Cheetah.Modules.NotesTimeline.Domain.Specifications;

namespace Cheetah.Modules.NotesTimeline.Application.Notes;

/// <summary>Активные заметки, привязанные к сущности (закреплённые — первыми, затем новые сверху).</summary>
public sealed record GetNotesByEntityQuery<TDto>(string EntityType, Guid EntityId) : IQuery<IReadOnlyList<TDto>>
    where TDto : NoteDtoBase;

public class GetNotesByEntityQueryHandler<TNote, TDto> : IQueryHandler<GetNotesByEntityQuery<TDto>, IReadOnlyList<TDto>>
    where TNote : NoteBase
    where TDto : NoteDtoBase
{
    private readonly IRepository<TNote, Guid> _repository;
    private readonly INoteProjector<TNote, TDto> _projector;

    public GetNotesByEntityQueryHandler(IRepository<TNote, Guid> repository, INoteProjector<TNote, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(
        GetNotesByEntityQuery<TDto> query, CancellationToken ct = default)
    {
        var spec = new NotesByEntitySpecification<TNote>(query.EntityType, query.EntityId);
        var notes = await _repository.GetAllAsync(spec, ct);

        return notes
            .OrderByDescending(n => n.PinnedAt.HasValue)
            .ThenByDescending(n => n.CreatedAt)
            .Select(_projector.ToDto)
            .ToArray();
    }
}
