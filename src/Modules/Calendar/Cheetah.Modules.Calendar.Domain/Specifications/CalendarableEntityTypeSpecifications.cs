using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Calendar.Domain.Entities;

namespace Cheetah.Modules.Calendar.Domain.Specifications;

/// <summary>Зарегистрированный привязываемый тип по его ключу.</summary>
public sealed class CalendarableTypeByKeySpecification : Specification<CalendarableEntityType>
{
    private readonly string _entityType;
    public CalendarableTypeByKeySpecification(string entityType) => _entityType = entityType;
    public override Expression<Func<CalendarableEntityType, bool>> ToExpression()
        => t => t.EntityType == _entityType;
}

/// <summary>Привязываемые типы по набору ключей (для пакетного upsert реестра одним запросом).</summary>
public sealed class CalendarableTypesByKeysSpecification : Specification<CalendarableEntityType>
{
    private readonly IReadOnlyCollection<string> _entityTypes;
    public CalendarableTypesByKeysSpecification(IReadOnlyCollection<string> entityTypes) => _entityTypes = entityTypes;
    public override Expression<Func<CalendarableEntityType, bool>> ToExpression()
        => t => _entityTypes.Contains(t.EntityType);
}
