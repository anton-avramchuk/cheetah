using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Calendar.Infrastructure.Persistence.Configurations;

public class EventAttendeeConfiguration : IEntityTypeConfiguration<EventAttendee>
{
    public void Configure(EntityTypeBuilder<EventAttendee> builder)
    {
        builder.ToTable("Attendees", CalendarConstants.Schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(CalendarConstants.MaxStatusLength).IsRequired();
        builder.Property(x => x.Response).HasConversion<string>().HasMaxLength(CalendarConstants.MaxStatusLength).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.EventId });
        builder.HasIndex(x => new { x.EventId, x.UserId }).IsUnique();
    }
}

public class EventReminderConfiguration : IEntityTypeConfiguration<EventReminder>
{
    public void Configure(EntityTypeBuilder<EventReminder> builder)
    {
        builder.ToTable("Reminders", CalendarConstants.Schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Target).HasConversion<string>().HasMaxLength(CalendarConstants.MaxStatusLength).IsRequired();
        builder.Property(x => x.ForceChannel).HasMaxLength(CalendarConstants.MaxStatusLength);
        builder.HasIndex(x => x.EventId);
    }
}

public class EventOccurrenceOverrideConfiguration : IEntityTypeConfiguration<EventOccurrenceOverride>
{
    public void Configure(EntityTypeBuilder<EventOccurrenceOverride> builder)
    {
        builder.ToTable("OccurrenceOverrides", CalendarConstants.Schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OccurrenceKey).HasMaxLength(CalendarConstants.MaxOccurrenceKeyLength).IsRequired();
        builder.Property(x => x.NewTitle).HasMaxLength(CalendarConstants.MaxTitleLength);
        builder.HasIndex(x => new { x.EventId, x.OccurrenceKey }).IsUnique();
    }
}

public class ReminderTriggerConfiguration : IEntityTypeConfiguration<ReminderTrigger>
{
    public void Configure(EntityTypeBuilder<ReminderTrigger> builder)
    {
        builder.ToTable("ReminderTriggers", CalendarConstants.Schema);
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);
        builder.Property(x => x.OccurrenceKey).HasMaxLength(CalendarConstants.MaxOccurrenceKeyLength).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(CalendarConstants.MaxStatusLength).IsRequired();

        // Горячий путь скана «пора слать».
        builder.HasIndex(x => new { x.Status, x.FireAtUtc });
        // Защита от дублей при пересчёте серии.
        builder.HasIndex(x => new { x.EventId, x.ReminderId, x.OccurrenceKey }).IsUnique();
    }
}

public class CalendarableEntityTypeConfiguration : IEntityTypeConfiguration<CalendarableEntityType>
{
    public void Configure(EntityTypeBuilder<CalendarableEntityType> builder)
    {
        builder.ToTable("CalendarableEntityTypes", CalendarConstants.Schema);
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);
        builder.Property(x => x.EntityType).HasMaxLength(CalendarConstants.MaxEntityTypeLength).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(CalendarConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.DefaultColor).HasMaxLength(CalendarConstants.MaxColorLength);
        builder.Property(x => x.OwnerService).HasMaxLength(CalendarConstants.MaxNameLength);
        builder.HasIndex(x => x.EntityType).IsUnique();
    }
}
