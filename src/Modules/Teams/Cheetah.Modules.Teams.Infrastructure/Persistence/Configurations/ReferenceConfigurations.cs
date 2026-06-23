using Cheetah.Modules.Teams.Domain.Entities;
using Cheetah.Modules.Teams.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Teams.Infrastructure.Persistence.Configurations;

/// <summary>Конкретная EF-конфигурация справочника ролей (уникальное имя).</summary>
public sealed class TeamRoleConfiguration : IEntityTypeConfiguration<TeamRole>
{
    public void Configure(EntityTypeBuilder<TeamRole> builder)
    {
        builder.ToTable(TeamsConstants.DefaultTeamRolesTableName, TeamsConstants.DefaultSchema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(TeamsConstants.MaxNameLength).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

/// <summary>Конкретная EF-конфигурация справочника участников.</summary>
public sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable(TeamsConstants.DefaultTeamMembersTableName, TeamsConstants.DefaultSchema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(TeamsConstants.MaxNameLength).IsRequired();
        builder.HasIndex(x => x.UserId);
    }
}

/// <summary>Конкретная EF-конфигурация членства (дитя команды): один участник — одна роль в команде.</summary>
public sealed class TeamMembershipConfiguration : IEntityTypeConfiguration<TeamMembership>
{
    public void Configure(EntityTypeBuilder<TeamMembership> builder)
    {
        builder.ToTable(TeamsConstants.DefaultTeamMembershipsTableName, TeamsConstants.DefaultSchema);
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TeamId, x.MemberId }).IsUnique();
        builder.HasIndex(x => x.MemberId);
    }
}
