using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Domain.Specifications;

/// <summary>Команда по имени. Используется для проверки уникальности при создании/переименовании.</summary>
public sealed class TeamByNameSpecification<TTeam> : Specification<TTeam>
    where TTeam : TeamBase
{
    private readonly string _name;

    public TeamByNameSpecification(string name) => _name = name;

    public override Expression<Func<TTeam, bool>> ToExpression()
        => t => t.Name == _name;
}

/// <summary>Команда по идентификатору вместе с составом (загрузка агрегата через Include).</summary>
public sealed class TeamByIdSpecification<TTeam> : Specification<TTeam>
    where TTeam : TeamBase
{
    private readonly Guid _id;

    public TeamByIdSpecification(Guid id) => _id = id;

    public override Expression<Func<TTeam, bool>> ToExpression()
        => t => t.Id == _id;
}

/// <summary>
/// Комбинированный фильтр списка команд. Любой критерий опционален (null = не учитывать). Поиск —
/// по подстроке в имени. Используется generic query-handler'ом вместо raw LINQ.
/// </summary>
public sealed class TeamsFilterSpecification<TTeam> : Specification<TTeam>
    where TTeam : TeamBase
{
    private readonly string? _search;
    private readonly bool _onlyActive;

    public TeamsFilterSpecification(string? search, bool onlyActive)
    {
        _search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        _onlyActive = onlyActive;
    }

    public override Expression<Func<TTeam, bool>> ToExpression()
        => t => (!_onlyActive || t.IsActive)
                && (_search == null || t.Name.Contains(_search));
}
