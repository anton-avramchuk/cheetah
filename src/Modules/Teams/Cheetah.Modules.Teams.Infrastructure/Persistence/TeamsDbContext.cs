using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Teams.Domain.Entities;
using Cheetah.Modules.Teams.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Teams.Infrastructure.Persistence;

[ConnectionStringName(TeamsConstants.DatabaseConnectionStringName)]
public abstract class
    TeamsDbContext<TContext, TTeam, TTeamRole, TTeamMember, TTeamMemberInTeamsValue> : CrmDbContext<TContext>
    where TContext : DbContext
    where TTeam : TeamBase
    where TTeamRole : TeamRoleBase
    where TTeamMember : TeamMember
    where TTeamMemberInTeamsValue : TeamMemberInTeamsValue<TTeam, TTeamMember, TTeamRole>
{
    protected TeamsDbContext(DbContextOptions<TContext> options) : base(options)
    {
    }

    public DbSet<TTeam> Teams => Set<TTeam>();

    public DbSet<TTeamRole> Roles => Set<TTeamRole>();

    public DbSet<TTeamMember> TeamMembers => Set<TTeamMember>();

    public DbSet<TTeamMemberInTeamsValue> MemberInTeamsValues => Set<TTeamMemberInTeamsValue>();
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(GetTeamConfiguration());
        modelBuilder.ApplyConfiguration(GetTeamRoleConfiguration());
        modelBuilder.ApplyConfiguration(GetTeamMemberConfiguration());
        modelBuilder.ApplyConfiguration(GetMemberInTeamsValuesConfiguration());
    }


    protected abstract IEntityTypeConfiguration<TTeam> GetTeamConfiguration();

    protected abstract IEntityTypeConfiguration<TTeamRole> GetTeamRoleConfiguration();

    protected abstract IEntityTypeConfiguration<TTeamMember> GetTeamMemberConfiguration();

    protected abstract IEntityTypeConfiguration<TTeamMemberInTeamsValue> GetMemberInTeamsValuesConfiguration();
}