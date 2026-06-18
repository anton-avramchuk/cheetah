using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Domain.Tests;

// Сущности модуля абстрактны (расширяемый шаблон) — тесты закрывают их минимальными sealed-наследниками,
// которые вызывают protected InitializeCore, как это сделал бы наследник модуля.

internal sealed class TestBookingType : BookingTypeBase
{
    public static TestBookingType New(
        int durationMinutes = 60, int maxAdvanceDays = 30, int minNoticeMinutes = 0,
        int bufferBefore = 0, int bufferAfter = 0, int? slotStep = null,
        Guid? host = null, string slug = "intro")
    {
        var t = new TestBookingType();
        t.InitializeCore(Guid.NewGuid(), host ?? Guid.NewGuid(), slug, "Intro call",
            durationMinutes, LocationKind.Video, maxAdvanceDays, minNoticeMinutes,
            bufferBefore, bufferAfter, slotStep);
        return t;
    }
}

internal sealed class TestSchedule : AvailabilityScheduleBase
{
    public static TestSchedule New(string timeZoneId = "UTC", Guid? host = null)
    {
        var s = new TestSchedule();
        s.InitializeCore(Guid.NewGuid(), host ?? Guid.NewGuid(), timeZoneId);
        return s;
    }
}

internal sealed class TestBooking : BookingBase
{
    public static TestBooking Reserve(
        BookingTypeBase type, DateTimeOffset startUtc,
        string name = "Jane", string email = "jane@example.com", string tz = "UTC")
    {
        var b = new TestBooking();
        b.InitializeCore(Guid.NewGuid(), type, startUtc, name, email, tz, null, Array.Empty<BookingAnswer>());
        return b;
    }
}
