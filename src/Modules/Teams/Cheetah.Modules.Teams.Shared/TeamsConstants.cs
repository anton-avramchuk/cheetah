namespace Cheetah.Modules.Teams.Shared;

/// <summary>
/// Общие константы шаблонного модуля Teams: имя БД-подключения, ограничения длин, дефолтные
/// имена таблиц/схемы и префиксы маршрутов. Используются абстрактными базами
/// (<c>TeamConfigurationBase</c>, <c>TeamEndpoints</c>) как значения по умолчанию, которые наследник
/// может переопределить.
/// </summary>
public static class TeamsConstants
{
    public const string DatabaseConnectionStringName = "Teams";

    public const int MaxNameLength = 256;

    public const string DefaultSchema = "teams";
    public const string DefaultTeamsTableName = "Teams";
    public const string DefaultTeamRolesTableName = "TeamRoles";
    public const string DefaultTeamMembersTableName = "TeamMembers";
    public const string DefaultTeamMembershipsTableName = "TeamMemberships";

    public const string DefaultTeamsRoutePrefix = "api/teams";
    public const string DefaultTeamRolesRoutePrefix = "api/team-roles";
    public const string DefaultTeamMembersRoutePrefix = "api/team-members";
}
