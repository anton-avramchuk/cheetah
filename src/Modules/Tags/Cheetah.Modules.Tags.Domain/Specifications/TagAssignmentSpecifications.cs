using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Tags.Domain.Entities;

namespace Cheetah.Modules.Tags.Domain.Specifications;

/// <summary>Все привязки конкретной сущности.</summary>
public sealed class AssignmentsByEntitySpecification : Specification<TagAssignment>
{
    private readonly string _entityType;
    private readonly Guid _entityId;
    public AssignmentsByEntitySpecification(string entityType, Guid entityId)
    {
        _entityType = entityType;
        _entityId = entityId;
    }
    public override Expression<Func<TagAssignment, bool>> ToExpression()
        => a => a.EntityType == _entityType && a.EntityId == _entityId;
}

/// <summary>Привязки сущности по подмножеству тэгов (для дедупликации и снятия).</summary>
public sealed class AssignmentsByEntityAndTagsSpecification : Specification<TagAssignment>
{
    private readonly string _entityType;
    private readonly Guid _entityId;
    private readonly IReadOnlyCollection<Guid> _tagIds;
    public AssignmentsByEntityAndTagsSpecification(
        string entityType, Guid entityId, IReadOnlyCollection<Guid> tagIds)
    {
        _entityType = entityType;
        _entityId = entityId;
        _tagIds = tagIds;
    }
    public override Expression<Func<TagAssignment, bool>> ToExpression()
        => a => a.EntityType == _entityType && a.EntityId == _entityId && _tagIds.Contains(a.TagId);
}
