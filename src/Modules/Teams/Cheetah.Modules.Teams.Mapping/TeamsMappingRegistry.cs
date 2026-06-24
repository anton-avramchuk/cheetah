using Cheetah.Mapping.Core;
using Cheetah.Modules.Teams.Application.Members;
using Cheetah.Modules.Teams.Application.Roles;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Mapping;

/// <summary>
/// Реестр генерируемых мапперов Teams (аналог Mapster-профиля, но через source generator).
/// Маркер размещён в выделенной маппинг-сборке, поэтому Contracts/Domain остаются чистыми.
/// Request → Command/Query — без проекции; Entity → Dto — read-проекции (нужен ProjectTo).
/// </summary>
// ── Roles ───────────────────────────────────────────────────────────────────
[GenerateMapper(typeof(CreateTeamRoleRequest), typeof(CreateTeamRoleCommand), GenerateProjection = false)]
[GenerateMapper(typeof(UpdateTeamRoleRequest), typeof(UpdateTeamRoleCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetTeamRoleByIdRequest), typeof(GetTeamRoleByIdQuery), GenerateProjection = false)]
[GenerateMapper(typeof(DeleteTeamRoleRequest), typeof(DeleteTeamRoleCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetTeamRolesGridRequest), typeof(GetTeamRolesGridQuery), GenerateProjection = false)]
[GenerateMapper(typeof(TeamRole), typeof(TeamRoleDto))]       // read-проекция: grid + GetById
// ── Members (только чтение) ───────────────────────────────────────────────────
[GenerateMapper(typeof(GetTeamMemberByIdRequest), typeof(GetTeamMemberByIdQuery), GenerateProjection = false)]
[GenerateMapper(typeof(GetTeamMembersGridRequest), typeof(GetTeamMembersGridQuery), GenerateProjection = false)]
[GenerateMapper(typeof(TeamMember), typeof(TeamMemberDto))]   // read-проекция: grid + GetById
public static partial class TeamsMappingRegistry
{
}
