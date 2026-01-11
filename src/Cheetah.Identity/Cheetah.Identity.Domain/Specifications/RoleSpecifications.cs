using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Identity.Domain.Entities;

namespace Cheetah.Identity.Domain.Specifications;

/// <summary>
/// Specification for finding role by name
/// </summary>
public class RoleByNameSpecification : Specification<Role>
{
    private readonly string _normalizedName;

    public RoleByNameSpecification(string name)
    {
        _normalizedName = name.ToUpperInvariant();
    }

    public override Expression<Func<Role, bool>> ToExpression()
    {
        return role => role.NormalizedName == _normalizedName;
    }
}

/// <summary>
/// Specification for finding role by ID
/// </summary>
public class RoleByIdSpecification : Specification<Role>
{
    private readonly Guid _roleId;

    public RoleByIdSpecification(Guid roleId)
    {
        _roleId = roleId;
    }

    public override Expression<Func<Role, bool>> ToExpression()
    {
        return role => role.Id == _roleId;
    }
}
