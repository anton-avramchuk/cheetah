using Cheetah.Modules.Booking.Default.Entities;
using Cheetah.Modules.Booking.Default.Persistence.Configurations;
using Cheetah.Modules.Booking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BookingEntity = Cheetah.Modules.Booking.Default.Entities.Booking;

namespace Cheetah.Modules.Booking.Default.Persistence;

/// <summary>Конкретный DbContext «из коробки» поверх абстрактной базы шаблона.</summary>
public sealed class BookingDbContext : BookingDbContextBase<BookingDbContext, BookingEntity, BookingType, AvailabilitySchedule>
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    protected override IEntityTypeConfiguration<BookingType> CreateBookingTypeConfiguration()
        => new BookingTypeConfiguration();

    protected override IEntityTypeConfiguration<AvailabilitySchedule> CreateScheduleConfiguration()
        => new AvailabilityScheduleConfiguration();

    protected override IEntityTypeConfiguration<BookingEntity> CreateBookingConfiguration()
        => new BookingConfiguration();
}
