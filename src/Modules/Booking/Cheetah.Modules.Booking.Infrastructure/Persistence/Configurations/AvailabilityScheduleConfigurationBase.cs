using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Booking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация расписания доступности. Недельные правила и исключения —
/// <b>owned-коллекции</b> (часть агрегата, грузятся автоматически вместе с расписанием — это нужно
/// слот-движку). Один host — одно расписание. Наследник расширяет схему через <see cref="ConfigureCustom"/>.
/// </summary>
public abstract class AvailabilityScheduleConfigurationBase<TSchedule> : IEntityTypeConfiguration<TSchedule>
    where TSchedule : AvailabilityScheduleBase
{
    protected virtual string TableName => BookingConstants.SchedulesTableName;
    protected virtual string Schema => BookingConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TSchedule> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TimeZoneId).HasMaxLength(BookingConstants.MaxTimeZoneLength).IsRequired();

        builder.OwnsMany(x => x.WeeklyRules, nb =>
        {
            nb.ToTable("WeeklyAvailabilityRules", Schema);
            nb.WithOwner().HasForeignKey(r => r.ScheduleId);
            nb.HasKey(r => r.Id);
            nb.Property(r => r.DayOfWeek).HasConversion<int>();
        });
        builder.OwnsMany(x => x.DateOverrides, nb =>
        {
            nb.ToTable("AvailabilityDateOverrides", Schema);
            nb.WithOwner().HasForeignKey(o => o.ScheduleId);
            nb.HasKey(o => o.Id);
        });

        builder.HasIndex(x => x.HostUserId).IsUnique();

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TSchedule> builder)
    {
    }
}
