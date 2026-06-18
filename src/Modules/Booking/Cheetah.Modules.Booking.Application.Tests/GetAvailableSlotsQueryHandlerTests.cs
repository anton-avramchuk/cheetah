using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Application.Slots;
using Cheetah.Modules.Booking.Domain.Slots;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Booking.Application.Tests;

public class GetAvailableSlotsQueryHandlerTests
{
    // Слот-хендлер использует реальный UtcNow для min-notice/горизонта — берём дату в будущем.
    private static readonly DateOnly Target = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(7);

    private static (Mock<IRepository<TestBookingType, Guid>>, Mock<IRepository<TestSchedule, Guid>>,
        Mock<IBookingCalendarGateway>) Mocks(TestBookingType? type, TestSchedule? schedule, BusyInterval[] busy)
    {
        var types = new Mock<IRepository<TestBookingType, Guid>>();
        types.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<TestBookingType>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(type);

        var schedules = new Mock<IRepository<TestSchedule, Guid>>();
        schedules.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<TestSchedule>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        var calendar = new Mock<IBookingCalendarGateway>();
        calendar.Setup(c => c.GetBusyAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(busy);

        return (types, schedules, calendar);
    }

    private static GetAvailableSlotsQueryHandler<TestBookingType, TestSchedule> Handler(
        Mock<IRepository<TestBookingType, Guid>> types, Mock<IRepository<TestSchedule, Guid>> schedules,
        Mock<IBookingCalendarGateway> calendar)
        => new(types.Object, schedules.Object, calendar.Object, new DefaultSlotEngine());

    [Fact]
    public async Task ReturnsRenderedSlots_WhenTypeAndScheduleExist()
    {
        var host = Guid.NewGuid();
        var type = TestBookingType.New(durationMinutes: 60, host: host);
        var schedule = TestSchedule.New("UTC", host);
        schedule.AddWeeklyRule(Target.DayOfWeek, new TimeOnly(9, 0), new TimeOnly(10, 0));

        var (types, schedules, calendar) = Mocks(type, schedule, Array.Empty<BusyInterval>());
        var result = await Handler(types, schedules, calendar)
            .HandleAsync(new GetAvailableSlotsQuery("intro", Target, Target, "UTC"));

        result.ShouldHaveSingleItem();
        result[0].StartUtc.ShouldBe(new DateTimeOffset(Target.ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero));
        result[0].InviteeLocalTime.ShouldEndWith("T09:00:00");
    }

    [Fact]
    public async Task ReturnsEmpty_WhenScheduleMissing()
    {
        var type = TestBookingType.New();
        var (types, schedules, calendar) = Mocks(type, null, Array.Empty<BusyInterval>());

        var result = await Handler(types, schedules, calendar)
            .HandleAsync(new GetAvailableSlotsQuery("intro", Target, Target, "UTC"));

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReturnsEmpty_WhenSlugUnknown()
    {
        var (types, schedules, calendar) = Mocks(null, null, Array.Empty<BusyInterval>());

        var result = await Handler(types, schedules, calendar)
            .HandleAsync(new GetAvailableSlotsQuery("nope", Target, Target, "UTC"));

        result.ShouldBeEmpty();
    }
}
