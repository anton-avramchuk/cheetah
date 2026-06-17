using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.NotesTimeline.Domain.Entities;

namespace Cheetah.Modules.NotesTimeline.Domain.Specifications;

/// <summary>
/// Строки ленты сущности с курсорной отсечкой (строки старше курсора <paramref name="before"/>) и
/// опциональным фильтром по видам. Используется query-handler'ом вместо raw LINQ.
/// </summary>
public sealed class TimelineByEntitySpecification : Specification<TimelineEntry>
{
    private readonly string _entityType;
    private readonly Guid _entityId;
    private readonly DateTimeOffset? _before;
    private readonly IReadOnlyCollection<string>? _kinds;

    public TimelineByEntitySpecification(
        string entityType, Guid entityId, DateTimeOffset? before, IReadOnlyCollection<string>? kinds)
    {
        _entityType = entityType;
        _entityId = entityId;
        _before = before;
        _kinds = kinds is { Count: > 0 } ? kinds : null;
    }

    public override Expression<Func<TimelineEntry, bool>> ToExpression()
        => e => e.EntityType == _entityType && e.EntityId == _entityId
                && (_before == null || e.OccurredAt < _before)
                && (_kinds == null || _kinds.Contains(e.Kind));
}

/// <summary>Строка ленты по идентификатору события-источника (проверка идемпотентности).</summary>
public sealed class TimelineEntryBySourceEventSpecification : Specification<TimelineEntry>
{
    private readonly string _sourceEventId;

    public TimelineEntryBySourceEventSpecification(string sourceEventId) => _sourceEventId = sourceEventId;

    public override Expression<Func<TimelineEntry, bool>> ToExpression()
        => e => e.SourceEventId == _sourceEventId;
}
