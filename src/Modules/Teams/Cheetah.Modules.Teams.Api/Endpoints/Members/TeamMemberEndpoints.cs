using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Teams.Application.Members;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Shared;

namespace Cheetah.Modules.Teams.Api.Endpoints.Members;

// Участники — read-only реплика пользователей Identity (наполняется фоновым синком), поэтому
// эндпоинтов на создание/обновление/удаление нет — только чтение.

public sealed class GetTeamMemberByIdEndpoint
    : QueryOrNotFoundEndpoint<GetTeamMemberByIdRequest, GetTeamMemberByIdQuery, TeamMemberDto, TeamMemberDto>
{
    public override string Route => $"{TeamsConstants.DefaultTeamMembersRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetTeamMemberById").WithTags("Teams.Members");
}

public sealed class GetTeamMembersGridEndpoint
    : QueryGridEndpoint<GetTeamMembersGridRequest, GetTeamMembersGridQuery, TeamMemberDto, TeamMemberDto>
{
    public override string Route => TeamsConstants.DefaultTeamMembersRoutePrefix;

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Members");
}
