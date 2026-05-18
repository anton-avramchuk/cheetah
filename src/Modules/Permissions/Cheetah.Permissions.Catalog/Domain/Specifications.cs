using System.Linq.Expressions;
using Cheetah.Core.Specification;

namespace Cheetah.Permissions.Catalog.Domain;

public sealed class PermissionsByModuleSpecification : Specification<PermissionDefinition>
{
    private readonly string _module;
    public PermissionsByModuleSpecification(string module) => _module = module;
    public override Expression<Func<PermissionDefinition, bool>> ToExpression()
        => p => p.Module == _module;
}

public sealed class PermissionsByKeysSpecification : Specification<PermissionDefinition>
{
    private readonly IReadOnlyCollection<string> _keys;
    public PermissionsByKeysSpecification(IReadOnlyCollection<string> keys) => _keys = keys;
    public override Expression<Func<PermissionDefinition, bool>> ToExpression()
        => p => _keys.Contains(p.Id);
}
