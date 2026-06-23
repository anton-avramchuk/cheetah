using Cheetah.Core.Domain;

namespace Cheetah.Modules.Teams.Domain.Entities;

public abstract class TeamMemberInTeamsValue<TTeam, TTeamMember, TTeamRole> : Entity
    where TTeamMember : TeamMember
    where TTeamRole : TeamRoleBase
    where TTeam : TeamBase
{
    private TeamMemberInTeamsValue()
    {
    }


    protected void InitializeCore(Guid teamId, Guid teamMemberId, Guid teamRoleId)
    {
        TeamId = teamId;
        TeamMemberId = teamMemberId;
        TeamRoleId = teamRoleId;
    }


    public Guid TeamId { get; set; }

    public TTeam Team { get; set; } = null!;

    public Guid TeamMemberId { get; set; }

    public TTeamMember TeamMember { get; set; } = null!;

    public Guid TeamRoleId { get; set; }

    public TTeamRole TeamRole { get; set; } = null!;


    public override object?[] GetKeys()
    {
        return [TeamId, TeamMemberId];
    }
}