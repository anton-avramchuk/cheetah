using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Tags.Domain.Entities;

namespace Cheetah.Modules.Tags.Domain.Specifications;

public sealed class TaggableTypesByKeysSpecification : Specification<TaggableEntityType>
{
    private readonly IReadOnlyCollection<string> _keys;
    public TaggableTypesByKeysSpecification(IReadOnlyCollection<string> keys) => _keys = keys;
    public override Expression<Func<TaggableEntityType, bool>> ToExpression()
        => t => _keys.Contains(t.Id);
}

public sealed class TaggableTypesByOwnerSpecification : Specification<TaggableEntityType>
{
    private readonly string _ownerService;
    public TaggableTypesByOwnerSpecification(string ownerService) => _ownerService = ownerService;
    public override Expression<Func<TaggableEntityType, bool>> ToExpression()
        => t => t.OwnerService == _ownerService;
}
