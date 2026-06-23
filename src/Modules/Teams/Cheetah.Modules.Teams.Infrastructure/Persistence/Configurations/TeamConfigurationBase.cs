using Cheetah.Modules.Teams.Domain.Entities;
using Cheetah.Modules.Teams.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Teams.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация команды: ключ, игнор доменных событий, общие колонки/индексы
/// и привязка коллекции состава (<see cref="TeamBase.Members"/>) с автозагрузкой. Наследник наследует
/// её и добавляет свои поля/индексы через <see cref="ConfigureCustom"/> — точка расширения схемы.
/// </summary>
public abstract class TeamConfigurationBase<TTeam> : IEntityTypeConfiguration<TTeam>
    where TTeam : TeamBase
{
    protected virtual string TableName => TeamsConstants.DefaultTeamsTableName;
    protected virtual string Schema => TeamsConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TTeam> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(TeamsConstants.MaxNameLength).IsRequired();

        builder.HasMany(x => x.Members)
            .WithOne()
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Members)
            .HasField("_members")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.IsActive);

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TTeam> builder)
    {
    }
}
