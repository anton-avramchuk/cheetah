using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.Abstractions;

/// <summary>
/// Проекция конкретной команды в конкретный DTO (включая состав и доп. поля наследника). Реализуется
/// наследником; используется generic query-handler'ами вместо Mapster, чтобы не требовать скрытой
/// конфигурации маппинга расширенных полей.
/// </summary>
public interface ITeamProjector<in TTeam, out TDto>
    where TTeam : TeamBase
    where TDto : TeamDtoBase
{
    TDto ToDto(TTeam team);
}
