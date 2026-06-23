using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Teams.Application.Roles;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Shared;

namespace Cheetah.Modules.Teams.Api.Endpoints.Roles;

public sealed class CreateTeamRoleEndpoint : CreateCommandEndpoint<CreateTeamRoleRequest, CreateTeamRoleCommand>
{
    public override string Route => TeamsConstants.DefaultTeamRolesRoutePrefix;
    public override string GetByIdRouteName => "GetTeamRoleById";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Roles");
}

public sealed class GetTeamRoleByIdEndpoint
    : QueryOrNotFoundEndpoint<GetTeamRoleByIdRequest, GetTeamRoleByIdQuery, TeamRoleDto, TeamRoleDto>
{
    public override string Route => $"{TeamsConstants.DefaultTeamRolesRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetTeamRoleById").WithTags("Teams.Roles");
}

public sealed class UpdateTeamRoleEndpoint : UpdateCommandEndpoint<UpdateTeamRoleRequest, UpdateTeamRoleCommand>
{
    public override string Route => $"{TeamsConstants.DefaultTeamRolesRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Roles");
}

public sealed class DeleteTeamRoleEndpoint : DeleteCommandEndpoint<DeleteTeamRoleRequest, DeleteTeamRoleCommand>
{
    public override string Route => $"{TeamsConstants.DefaultTeamRolesRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Roles");
}

public sealed class GetTeamRolesGridEndpoint
    : QueryGridEndpoint<GetTeamRolesGridRequest, GetTeamRolesGridQuery, TeamRoleDto, TeamRoleDto>
{
    public override string Route => TeamsConstants.DefaultTeamRolesRoutePrefix;

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Roles");
}
