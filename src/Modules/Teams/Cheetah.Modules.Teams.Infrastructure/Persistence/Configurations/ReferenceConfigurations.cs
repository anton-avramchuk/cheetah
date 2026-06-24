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

/// <summary>Конкретная EF-конфигурация справочника участников (реплика пользователей Identity).</summary>
public sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable(TeamsConstants.DefaultTeamMembersTableName, TeamsConstants.DefaultSchema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever(); // id задаётся приложением/Identity

        builder.Property(x => x.Name).HasMaxLength(TeamsConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.SyncHash).HasMaxLength(TeamsConstants.SyncHashLength).IsRequired(); // SHA-256 hex
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
        // индексы по MemberId/RoleId EF создаёт по конвенции для FK ниже

        // FK на участника и роль — без навигаций (агрегаты в коде остаются независимыми), но БД
        // обеспечивает референсную целостность и каскад. Связь с командой задаётся в
        // TeamConfigurationBase (HasMany(Members)). PostgreSQL допускает несколько каскадных путей.
        builder.HasOne<TeamMember>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade); // удалили участника → его членства уходят автоматически

        builder.HasOne<TeamRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // роль, занятую в командах, удалить нельзя
    }
}
