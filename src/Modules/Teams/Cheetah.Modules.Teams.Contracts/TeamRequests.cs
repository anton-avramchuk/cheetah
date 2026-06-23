using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Cheetah.Modules.Teams.Contracts;

/// <summary>
/// Базовый запрос на создание команды. Абстрактен: наследник объявляет конкретный
/// <c>sealed record CreateTeamRequest : CreateTeamRequestBase</c>, добавляет свои поля и
/// (опционально) атрибут <c>[ApiRoute(..., ApiMethod.Create)]</c>.
/// </summary>
public abstract record CreateTeamRequestBase : ICrmRequest
{
    public string Name { get; init; } = null!;
}

/// <summary>Базовый запрос на переименование команды (Id — из маршрута).</summary>
public abstract record UpdateTeamRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
    public string Name { get; init; } = null!;
}

/// <summary>Запрос «команда по Id».</summary>
public abstract record GetTeamByIdRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
}

/// <summary>Запрос «удалить команду».</summary>
public abstract record DeleteTeamRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
}

/// <summary>Запрос грида команд (пагинация/сортировка/фильтрация). Наследник — конкретный class.</summary>
public abstract class GetTeamsGridRequestBase : GridRequest;

/// <summary>Базовый запрос «добавить участника в команду».</summary>
public abstract record AddTeamMemberRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public Guid RoleId { get; init; }
}

/// <summary>Базовый запрос «изменить роль участника» (Id команды и MemberId — из маршрута).</summary>
public abstract record ChangeTeamMemberRoleRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
    [FromRoute] public Guid MemberId { get; init; }
    public Guid RoleId { get; init; }
}

/// <summary>Базовый запрос «удалить участника из команды».</summary>
public abstract record RemoveTeamMemberRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
    [FromRoute] public Guid MemberId { get; init; }
}
