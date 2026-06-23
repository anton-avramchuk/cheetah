using Cheetah.Core.EntityFramework.Configuration;
using Cheetah.Modules.Teams.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Teams.Infrastructure.Persistence.Configurations;

public abstract class
    TeamMemberInTeamsConfiguration<TTeam, TTeamMember, TTeamRole> : IEntityTypeConfiguration<
    TeamMemberInTeamsValue<TTeam, TTeamMember, TTeamRole>> where TTeam : TeamBase
    where TTeamMember : TeamMember
    where TTeamRole : TeamRoleBase
{
    public void Configure(EntityTypeBuilder<TeamMemberInTeamsValue<TTeam, TTeamMember, TTeamRole>> builder)
    {
        builder.ToTable("TeamMemberInTeams", "teams");

        builder.HasKey(x => new { x.TeamId, x.TeamMemberId });

        builder.HasIndex(x => new { x.TeamId, x.TeamMemberId, x.TeamRoleId }).IsUnique();
    }
}