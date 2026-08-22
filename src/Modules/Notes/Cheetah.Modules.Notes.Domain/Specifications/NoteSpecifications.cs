using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Notes.Domain.Entities;

namespace Cheetah.Modules.Notes.Domain.Specifications;

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

/// <summary>Активные заметки автора (лента «мои заметки», проверки авторства).</summary>
public sealed class NotesByAuthorSpecification<TNote> : Specification<TNote>
    where TNote : NoteBase
{
    private readonly Guid _authorId;

    public NotesByAuthorSpecification(Guid authorId) => _authorId = authorId;

    public override Expression<Func<TNote, bool>> ToExpression()
        => n => n.AuthorId == _authorId && n.RemovedAt == null;
}

/// <summary>Ветка треда — активные ответы на заметку.</summary>
public sealed class NoteThreadSpecification<TNote> : Specification<TNote>
    where TNote : NoteBase
{
    private readonly Guid _parentNoteId;

    public NoteThreadSpecification(Guid parentNoteId) => _parentNoteId = parentNoteId;

    public override Expression<Func<TNote, bool>> ToExpression()
        => n => n.ParentNoteId == _parentNoteId && n.RemovedAt == null;
}
