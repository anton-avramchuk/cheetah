using Cheetah.Modules.Activities.Domain.Entities;
using Cheetah.Modules.Activities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Activities.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация активности: ключ, игнор доменных событий, общие колонки,
/// связь с напоминаниями и индексы под горячие пути. Наследник наследует её и добавляет свои
/// поля/индексы через <see cref="ConfigureCustom"/> — точка расширения схемы.
/// </summary>
public abstract class ActivityConfigurationBase<TActivity> : IEntityTypeConfiguration<TActivity>
    where TActivity : ActivityBase
{
    protected virtual string TableName => ActivitiesConstants.DefaultTableName;
    protected virtual string Schema => ActivitiesConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TActivity> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(ActivitiesConstants.MaxTitleLength).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(ActivitiesConstants.MaxEntityTypeLength).IsRequired();
        builder.Property(x => x.Result).HasMaxLength(ActivitiesConstants.MaxResultLength);
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Priority).HasConversion<int>();
        builder.Property(x => x.Attributes).HasColumnType("jsonb");

        builder.HasMany(x => x.Reminders)
            .WithOne()
            .HasForeignKey(r => r.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Reminders)
            .HasField("_reminders")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Горячие пути: «мои открытые», «активности сущности», скан просрочек.
        builder.HasIndex(x => new { x.AssigneeId, x.Status });
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.Status, x.DueAt });

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TActivity> builder)
    {
    }
}
