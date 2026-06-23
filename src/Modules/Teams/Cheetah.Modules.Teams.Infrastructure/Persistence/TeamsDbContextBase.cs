using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Teams.Domain.Entities;
using Cheetah.Modules.Teams.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.Teams.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Teams.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля Teams. Хостит команды
/// (<typeparamref name="TTeam"/>) с составом, справочники ролей и участников в одной БД. Наследник
/// закрывает его конкретным типом команды:
/// <c>class AppTeamsDbContext : TeamsDbContextBase&lt;AppTeamsDbContext, Team&gt;</c>
/// и поставляет конфигурацию команды через <see cref="CreateTeamConfiguration"/>. Миграции — у
/// наследника. Имя подключения по умолчанию — <see cref="TeamsConstants.DatabaseConnectionStringName"/>.
/// </summary>
[ConnectionStringName(TeamsConstants.DatabaseConnectionStringName)]
public abstract class TeamsDbContextBase<TContext, TTeam> : CrmDbContext<TContext>
    where TContext : DbContext
    where TTeam : TeamBase
{
    public DbSet<TTeam> Teams => Set<TTeam>();
    public DbSet<TeamRole> Roles => Set<TeamRole>();
    public DbSet<TeamMember> Members => Set<TeamMember>();
    public DbSet<TeamMembership> Memberships => Set<TeamMembership>();

    protected TeamsDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(CreateTeamConfiguration());
        modelBuilder.ApplyConfiguration(new TeamRoleConfiguration());
        modelBuilder.ApplyConfiguration(new TeamMemberConfiguration());
        modelBuilder.ApplyConfiguration(new TeamMembershipConfiguration());
    }

    /// <summary>Конкретная конфигурация сущности команды, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TTeam> CreateTeamConfiguration();
}
