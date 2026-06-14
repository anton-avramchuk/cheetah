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
