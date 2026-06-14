using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.Calendar.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Calendar.Infrastructure.Persistence;

/// <summary>
/// БД модуля Calendar. Реализует <see cref="IOutboxDbContext"/> + <see cref="IDeadLetterDbContext"/>:
/// намерения уведомить (NotificationRequested), опубликованные через OutboxEventBus, ложатся в
/// OutboxMessages в той же транзакции, что и гашение триггеров — атомарность «отметил + опубликовал».
/// </summary>
[ConnectionStringName(CalendarConstants.ConnectionStringName)]
public class CalendarDbContext(DbContextOptions<CalendarDbContext> options)
    : CrmDbContext<CalendarDbContext>(options), IOutboxDbContext, IDeadLetterDbContext
{
    public DbSet<Domain.Entities.Calendar> Calendars => Set<Domain.Entities.Calendar>();
    public DbSet<CalendarEvent> Events => Set<CalendarEvent>();
    public DbSet<EventAttendee> Attendees => Set<EventAttendee>();
    public DbSet<EventReminder> Reminders => Set<EventReminder>();
    public DbSet<EventOccurrenceOverride> OccurrenceOverrides => Set<EventOccurrenceOverride>();
    public DbSet<ReminderTrigger> ReminderTriggers => Set<ReminderTrigger>();
    public DbSet<CalendarableEntityType> CalendarableEntityTypes => Set<CalendarableEntityType>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<DeadLetterMessage> DeadLetterMessages => Set<DeadLetterMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CalendarConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarEventConfiguration());
        modelBuilder.ApplyConfiguration(new EventAttendeeConfiguration());
        modelBuilder.ApplyConfiguration(new EventReminderConfiguration());
        modelBuilder.ApplyConfiguration(new EventOccurrenceOverrideConfiguration());
        modelBuilder.ApplyConfiguration(new ReminderTriggerConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarableEntityTypeConfiguration());

        modelBuilder.AddOutbox();
        modelBuilder.AddDeadLetter();
    }
}
