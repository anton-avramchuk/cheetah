using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Activities.Domain.Entities;
using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Domain.Specifications;

/// <summary>Активности, привязанные к конкретной сущности <c>(EntityType, EntityId)</c>.</summary>
public sealed class ActivitiesByEntitySpecification<TActivity> : Specification<TActivity>
    where TActivity : ActivityBase
{
    private readonly string _entityType;
    private readonly Guid _entityId;

    public ActivitiesByEntitySpecification(string entityType, Guid entityId)
    {
        _entityType = entityType;
        _entityId = entityId;
    }

    public override Expression<Func<TActivity, bool>> ToExpression()
        => a => a.EntityType == _entityType && a.EntityId == _entityId;
}

/// <summary>Незакрытые активности (Open/InProgress) указанного исполнителя.</summary>
public sealed class OpenActivitiesByAssigneeSpecification<TActivity> : Specification<TActivity>
    where TActivity : ActivityBase
{
    private readonly Guid _assigneeId;

    public OpenActivitiesByAssigneeSpecification(Guid assigneeId) => _assigneeId = assigneeId;

    public override Expression<Func<TActivity, bool>> ToExpression()
        => a => a.AssigneeId == _assigneeId &&
                (a.Status == ActivityStatus.Open || a.Status == ActivityStatus.InProgress);
}

/// <summary>Открытые активности, у которых истёк срок (для фонового скана просрочек).</summary>
public sealed class OverdueActivitiesSpecification<TActivity> : Specification<TActivity>
    where TActivity : ActivityBase
{
    private readonly DateTimeOffset _now;

    public OverdueActivitiesSpecification(DateTimeOffset now) => _now = now;

    public override Expression<Func<TActivity, bool>> ToExpression()
        => a => a.Status == ActivityStatus.Open && a.DueAt != null && a.DueAt < _now;
}

/// <summary>
/// Комбинированный фильтр списка активностей. Любой из критериев опционален (null = не учитывать).
/// Используется generic query-handler'ом вместо raw LINQ.
/// </summary>
public sealed class ActivitiesFilterSpecification<TActivity> : Specification<TActivity>
    where TActivity : ActivityBase
{
    private readonly Guid? _assigneeId;
    private readonly ActivityStatus? _status;
    private readonly string? _entityType;
    private readonly Guid? _entityId;
    private readonly DateTimeOffset? _dueBefore;

    public ActivitiesFilterSpecification(
        Guid? assigneeId, ActivityStatus? status, string? entityType, Guid? entityId, DateTimeOffset? dueBefore)
    {
        _assigneeId = assigneeId;
        _status = status;
        _entityType = entityType;
        _entityId = entityId;
        _dueBefore = dueBefore;
    }

    public override Expression<Func<TActivity, bool>> ToExpression()
        => a => (_assigneeId == null || a.AssigneeId == _assigneeId)
                && (_status == null || a.Status == _status)
                && (_entityType == null || a.EntityType == _entityType)
                && (_entityId == null || a.EntityId == _entityId)
                && (_dueBefore == null || (a.DueAt != null && a.DueAt < _dueBefore));
}
