using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.NotesTimeline.Domain.Entities;

namespace Cheetah.Modules.NotesTimeline.Domain.Specifications;

/// <summary>Активные (не удалённые) заметки, привязанные к сущности <c>(EntityType, EntityId)</c>.</summary>
public sealed class NotesByEntitySpecification<TNote> : Specification<TNote>
    where TNote : NoteBase
{
    private readonly string _entityType;
    private readonly Guid _entityId;

    public NotesByEntitySpecification(string entityType, Guid entityId)
    {
        _entityType = entityType;
        _entityId = entityId;
    }

    public override Expression<Func<TNote, bool>> ToExpression()
        => n => n.EntityType == _entityType && n.EntityId == _entityId && n.RemovedAt == null;
}

/// <summary>Закреплённые активные заметки сущности.</summary>
public sealed class PinnedNotesByEntitySpecification<TNote> : Specification<TNote>
    where TNote : NoteBase
{
    private readonly string _entityType;
    private readonly Guid _entityId;

    public PinnedNotesByEntitySpecification(string entityType, Guid entityId)
    {
        _entityType = entityType;
        _entityId = entityId;
    }

    public override Expression<Func<TNote, bool>> ToExpression()
        => n => n.EntityType == _entityType && n.EntityId == _entityId
                && n.RemovedAt == null && n.PinnedAt != null;
}
