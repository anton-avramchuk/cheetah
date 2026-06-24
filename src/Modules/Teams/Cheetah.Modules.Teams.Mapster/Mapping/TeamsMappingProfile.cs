using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Teams.Application.Members;
using Cheetah.Modules.Teams.Application.Roles;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;
using Mapster;

namespace Cheetah.Modules.Teams.Mapster.Mapping;

/// <summary>
/// Mapster-маппинги модуля Teams: Request → Command/Query и Entity → ViewModel (для ProjectTo в гриде
/// и GetById). Покрывает конкретные справочники — роли и участники. Маппинги расширяемой команды
/// (Request → CreateTeamCommand&lt;…&gt;, Team → ViewModel) объявляет наследник/хост.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public sealed class TeamsMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // ── Roles ──────────────────────────────────────────────────────────────────────────────
        config.NewConfig<CreateTeamRoleRequest, CreateTeamRoleCommand>();
        config.NewConfig<UpdateTeamRoleRequest, UpdateTeamRoleCommand>();
        config.NewConfig<GetTeamRoleByIdRequest, GetTeamRoleByIdQuery>();
        config.NewConfig<DeleteTeamRoleRequest, DeleteTeamRoleCommand>();
        config.NewConfig<GetTeamRolesGridRequest, GetTeamRolesGridQuery>();
        config.NewConfig<TeamRole, TeamRoleDto>();           // ProjectTo: grid + GetById

        // ── Members (только чтение — наполняются из Identity) ────────────────────────────────────
        config.NewConfig<GetTeamMemberByIdRequest, GetTeamMemberByIdQuery>();
        config.NewConfig<GetTeamMembersGridRequest, GetTeamMembersGridQuery>();
        config.NewConfig<TeamMember, TeamMemberDto>();        // ProjectTo: grid + GetById
    }
}
