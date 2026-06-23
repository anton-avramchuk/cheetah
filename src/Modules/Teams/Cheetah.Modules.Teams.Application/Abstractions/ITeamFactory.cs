using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.Abstractions;

/// <summary>
/// Фабрика конкретной команды из запроса на создание. Реализуется наследником — он знает, как
/// сконструировать свою сущность (включая доп. поля) и завести инварианты/события через
/// <c>InitializeCore</c>. Так generic-handler создаёт команду, не зная конкретного типа.
/// </summary>
public interface ITeamFactory<out TTeam, in TCreateRequest>
    where TTeam : TeamBase
    where TCreateRequest : CreateTeamRequestBase
{
    TTeam Create(TCreateRequest request);
}
