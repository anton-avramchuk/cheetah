using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Tags.Domain.Entities;

namespace Cheetah.Modules.Tags.Domain.Specifications;

public sealed class TagByNameSpecification : Specification<Tag>
{
    private readonly string _name;
    public TagByNameSpecification(string name) => _name = name;
    public override Expression<Func<Tag, bool>> ToExpression()
        => t => t.Name == _name;
}

public sealed class TagsByGroupSpecification : Specification<Tag>
{
    private readonly string _group;
    public TagsByGroupSpecification(string group) => _group = group;
    public override Expression<Func<Tag, bool>> ToExpression()
        => t => t.Group == _group;
}

public sealed class TagsByIdsSpecification : Specification<Tag>
{
    private readonly IReadOnlyCollection<Guid> _ids;
    public TagsByIdsSpecification(IReadOnlyCollection<Guid> ids) => _ids = ids;
    public override Expression<Func<Tag, bool>> ToExpression()
        => t => _ids.Contains(t.Id);
}
