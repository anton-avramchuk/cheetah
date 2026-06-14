using Cheetah.Modules.Calendar.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Calendar.Infrastructure.Persistence.Configurations;

public class CalendarConfiguration : IEntityTypeConfiguration<Domain.Entities.Calendar>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Calendar> builder)
    {
        builder.ToTable("Calendars", CalendarConstants.Schema);
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.Name).HasMaxLength(CalendarConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(CalendarConstants.MaxStatusLength).IsRequired();
        builder.Property(x => x.DefaultTimeZoneId).HasMaxLength(CalendarConstants.MaxTimeZoneLength).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(CalendarConstants.MaxColorLength);

        builder.HasIndex(x => x.OwnerUserId);
    }
}
