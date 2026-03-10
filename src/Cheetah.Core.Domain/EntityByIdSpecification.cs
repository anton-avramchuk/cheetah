using System.Linq.Expressions;
using Cheetah.Core.Specification;

namespace Cheetah.Core.Domain;

public class EntityByIdSpecification<T>(Guid id) : Specification<T>
    where T : Entity<Guid>
{
    public override Expression<Func<T, bool>> ToExpression() => e => e.Id == id;
}
