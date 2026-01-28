using System.Linq.Expressions;
using Cheetah.Core.Domain;
using Cheetah.Core.Specification;

namespace Cheetah.Core.DataAccess.Specifications;

public class EntityByIdSpecification<TEntity, TKey> : Specification<TEntity>
    where TEntity : Entity<TKey>
{
    private readonly TKey _id;

    public EntityByIdSpecification(TKey id) => _id = id;

    public override Expression<Func<TEntity, bool>> ToExpression()
        => entity => entity.Id!.Equals(_id);
}
