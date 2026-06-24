using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Domain.Specifications;

/// <summary>Роль по имени (проверка уникальности справочника ролей).</summary>
public sealed class TeamRoleByNameSpecification : Specification<TeamRole>
{
    private readonly string _name;

    public TeamRoleByNameSpecification(string name) => _name = name;

    public override Expression<Func<TeamRole, bool>> ToExpression()
        => r => r.Name == _name;
}
