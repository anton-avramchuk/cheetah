using System.Text.Json;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cheetah.Modules.Calendar.Infrastructure.Persistence.Configurations;

public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable("Events", CalendarConstants.Schema);
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.IsRecurring);

        builder.Property(x => x.Title).HasMaxLength(CalendarConstants.MaxTitleLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(CalendarConstants.MaxDescriptionLength);
        builder.Property(x => x.Location).HasMaxLength(CalendarConstants.MaxLocationLength);
        builder.Property(x => x.TimeZoneId).HasMaxLength(CalendarConstants.MaxTimeZoneLength).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(CalendarConstants.MaxEntityTypeLength);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(CalendarConstants.MaxStatusLength).IsRequired();

        // Правило повторения — опциональный owned-тип; null → колонки NULL (разовое событие).
        var exDatesConverter = new ValueConverter<IReadOnlyList<DateTime>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<DateTime>>(v, (JsonSerializerOptions?)null) ?? new List<DateTime>());

        var exDatesComparer = new ValueComparer<IReadOnlyList<DateTime>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (h, d) => HashCode.Combine(h, d.GetHashCode())),
            v => v.ToList());

        builder.OwnsOne(x => x.Recurrence, r =>
        {
            r.Property(p => p.RRule).HasColumnName("RecurrenceRRule").HasMaxLength(CalendarConstants.MaxRRuleLength);
            r.Property(p => p.ExDatesUtc).HasColumnName("RecurrenceExDates")
                .HasConversion(exDatesConverter, exDatesComparer);
        });
        builder.Navigation(x => x.Recurrence).IsRequired(false);

        builder.HasMany(x => x.Attendees).WithOne().HasForeignKey(a => a.EventId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Reminders).WithOne().HasForeignKey(r => r.EventId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Overrides).WithOne().HasForeignKey(o => o.EventId).OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Attendees).HasField("_attendees").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Reminders).HasField("_reminders").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Overrides).HasField("_overrides").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new { x.CalendarId, x.StartUtc });
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
