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

/// <summary>Участник по ссылке на пользователя Identity (поиск уже заведённого участника).</summary>
public sealed class TeamMemberByUserSpecification : Specification<TeamMember>
{
    private readonly Guid _userId;

    public TeamMemberByUserSpecification(Guid userId) => _userId = userId;

    public override Expression<Func<TeamMember, bool>> ToExpression()
        => m => m.UserId == _userId;
}
