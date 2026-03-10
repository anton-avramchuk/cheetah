using System.Linq.Expressions;
using Cheetah.Core.Domain;
using Cheetah.Core.Specification;

namespace Crm.VacancyTasks.Domain.Specifications;

public class EntityByIdSpecification<T>(Guid id) : Specification<T>
    where T : Entity<Guid>
{
    public override Expression<Func<T, bool>> ToExpression() => e => e.Id == id;
}
