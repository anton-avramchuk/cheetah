using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Modules.Teams.Shared;

namespace Cheetah.Modules.Teams.Contracts;

// ── ViewModel (конкретный — справочник участников не расширяется) ───────────────────────────────

/// <summary>ViewModel участника (справочник людей). Используется и для грида, и для детали.</summary>
public sealed record TeamMemberDto : ICrmResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public Guid? UserId { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

// ── Requests ───────────────────────────────────────────────────────────────────────────────────

[ApiRoute(TeamsConstants.DefaultTeamMembersRoutePrefix, ApiMethod.Create, ServiceName = "TeamMembers")]
public sealed record CreateTeamMemberRequest(string Name, Guid? UserId = null) : ICrmRequest;

[ApiRoute(TeamsConstants.DefaultTeamMembersRoutePrefix + "/{id:guid}", ApiMethod.Update, ServiceName = "TeamMembers")]
public sealed record UpdateTeamMemberRequest([FromRoute] Guid Id, string Name, Guid? UserId = null) : ICrmRequest;

[ApiRoute(TeamsConstants.DefaultTeamMembersRoutePrefix + "/{id:guid}", ApiMethod.GetOrNotFound,
    ResponseType = typeof(TeamMemberDto), ServiceName = "TeamMembers")]
public sealed record GetTeamMemberByIdRequest([FromRoute] Guid Id) : ICrmRequest;

[ApiRoute(TeamsConstants.DefaultTeamMembersRoutePrefix + "/{id:guid}", ApiMethod.Delete, ServiceName = "TeamMembers")]
public sealed record DeleteTeamMemberRequest([FromRoute] Guid Id) : ICrmRequest;

[ApiRoute(TeamsConstants.DefaultTeamMembersRoutePrefix, ApiMethod.GetGrid,
    ResponseType = typeof(TeamMemberDto), ServiceName = "TeamMembers")]
public sealed class GetTeamMembersGridRequest : GridRequest;
