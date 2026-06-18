using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Default.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;

namespace Cheetah.Modules.Booking.Default.Entities;

/// <summary>Конкретный тип встречи «из коробки» (без доп. полей). Наследник модуля делает свой.</summary>
public sealed class BookingType : BookingTypeBase
{
    public static BookingType Create(CreateBookingTypeRequest r)
    {
        var type = new BookingType();
        type.InitializeCore(Guid.NewGuid(), r.HostUserId, r.Slug, r.Name, r.DurationMinutes, r.LocationKind,
            r.MaxAdvanceDays, r.MinNoticeMinutes, r.BufferBeforeMinutes, r.BufferAfterMinutes,
            r.SlotStepMinutes, r.LocationDetails, r.Color);
        return type;
    }
}

/// <summary>Конкретное расписание доступности «из коробки».</summary>
public sealed class AvailabilitySchedule : AvailabilityScheduleBase
{
    public static AvailabilitySchedule Create(Guid hostUserId, string timeZoneId)
    {
        var schedule = new AvailabilitySchedule();
        schedule.InitializeCore(Guid.NewGuid(), hostUserId, timeZoneId);
        return schedule;
    }
}

/// <summary>Конкретная бронь «из коробки».</summary>
public sealed class Booking : BookingBase
{
    public static Booking Reserve(BookingTypeBase type, DateTimeOffset startUtc, CreatePublicBookingRequest r)
    {
        var booking = new Booking();
        booking.InitializeCore(Guid.NewGuid(), type, startUtc, r.InviteeName, r.InviteeEmail, r.InviteeTimeZone,
            r.InviteePhone, r.Answers.Select(a => BookingAnswer.Create(a.Question, a.Value)));
        return booking;
    }
}
