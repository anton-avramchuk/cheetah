using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Modules.Teams.Shared;

namespace Cheetah.Modules.Teams.Contracts;

// ── ViewModel (конкретный — роли не расширяются) ───────────────────────────────────────────────

/// <summary>ViewModel роли участника. Используется и для грида, и для детали.</summary>
public sealed record TeamRoleDto : ICrmResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

// ── Requests ───────────────────────────────────────────────────────────────────────────────────

[ApiRoute(TeamsConstants.DefaultTeamRolesRoutePrefix, ApiMethod.Create, ServiceName = "TeamRoles")]
public sealed record CreateTeamRoleRequest(string Name) : ICrmRequest;

[ApiRoute(TeamsConstants.DefaultTeamRolesRoutePrefix + "/{id:guid}", ApiMethod.Update, ServiceName = "TeamRoles")]
public sealed record UpdateTeamRoleRequest([FromRoute] Guid Id, string Name) : ICrmRequest;

[ApiRoute(TeamsConstants.DefaultTeamRolesRoutePrefix + "/{id:guid}", ApiMethod.GetOrNotFound,
    ResponseType = typeof(TeamRoleDto), ServiceName = "TeamRoles")]
public sealed record GetTeamRoleByIdRequest([FromRoute] Guid Id) : ICrmRequest;

[ApiRoute(TeamsConstants.DefaultTeamRolesRoutePrefix + "/{id:guid}", ApiMethod.Delete, ServiceName = "TeamRoles")]
public sealed record DeleteTeamRoleRequest([FromRoute] Guid Id) : ICrmRequest;

[ApiRoute(TeamsConstants.DefaultTeamRolesRoutePrefix, ApiMethod.GetGrid,
    ResponseType = typeof(TeamRoleDto), ServiceName = "TeamRoles")]
public sealed class GetTeamRolesGridRequest : GridRequest;
