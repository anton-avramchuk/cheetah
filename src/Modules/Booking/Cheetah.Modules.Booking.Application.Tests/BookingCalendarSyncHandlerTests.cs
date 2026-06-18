using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Application.Bookings;
using Cheetah.Modules.Booking.DomainEvents;
using Moq;

namespace Cheetah.Modules.Booking.Application.Tests;

public class BookingCalendarSyncHandlerTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 5, 9, 0, 0, TimeSpan.Zero);

    private static (Mock<IRepository<TestBooking, Guid>>, Mock<IBookingCalendarGateway>) Mocks(TestBooking? booking)
    {
        var repo = new Mock<IRepository<TestBooking, Guid>>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        return (repo, new Mock<IBookingCalendarGateway>());
    }

    [Fact]
    public async Task Cancelled_WithCalendarEvent_CancelsIt()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(), Start);
        var eventId = Guid.NewGuid();
        booking.AttachCalendarEvent(eventId);
        var (repo, gw) = Mocks(booking);

        await new BookingCancelledCalendarSyncHandler<TestBooking>(repo.Object, gw.Object)
            .HandleAsync(new BookingCancelledIntegrationEvent(booking.Id, "x", true));

        gw.Verify(c => c.CancelEventAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancelled_WithoutCalendarEvent_DoesNothing()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(), Start);
        var (repo, gw) = Mocks(booking);

        await new BookingCancelledCalendarSyncHandler<TestBooking>(repo.Object, gw.Object)
            .HandleAsync(new BookingCancelledIntegrationEvent(booking.Id, "x", true));

        gw.Verify(c => c.CancelEventAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancelled_BookingNotFound_DoesNothing()
    {
        var (repo, gw) = Mocks(null);

        await new BookingCancelledCalendarSyncHandler<TestBooking>(repo.Object, gw.Object)
            .HandleAsync(new BookingCancelledIntegrationEvent(Guid.NewGuid(), "x", true));

        gw.Verify(c => c.CancelEventAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Rescheduled_WithCalendarEvent_MovesIt()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(durationMinutes: 60), Start);
        var eventId = Guid.NewGuid();
        booking.AttachCalendarEvent(eventId);
        var (repo, gw) = Mocks(booking);

        var newStart = Start.AddDays(1);
        var newEnd = newStart.AddMinutes(60);
        await new BookingRescheduledCalendarSyncHandler<TestBooking>(repo.Object, gw.Object)
            .HandleAsync(new BookingRescheduledIntegrationEvent(booking.Id, newStart, newEnd));

        gw.Verify(c => c.RescheduleEventAsync(eventId, newStart, newEnd, It.IsAny<CancellationToken>()), Times.Once);
    }
}
