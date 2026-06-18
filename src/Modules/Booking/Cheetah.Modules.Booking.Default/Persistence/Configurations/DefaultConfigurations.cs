using Cheetah.Modules.Booking.Default.Entities;
using Cheetah.Modules.Booking.Infrastructure.Persistence.Configurations;
using BookingEntity = Cheetah.Modules.Booking.Default.Entities.Booking;

namespace Cheetah.Modules.Booking.Default.Persistence.Configurations;

public sealed class BookingTypeConfiguration : BookingTypeConfigurationBase<BookingType>;

public sealed class AvailabilityScheduleConfiguration : AvailabilityScheduleConfigurationBase<AvailabilitySchedule>;

public sealed class BookingConfiguration : BookingConfigurationBase<BookingEntity>;
