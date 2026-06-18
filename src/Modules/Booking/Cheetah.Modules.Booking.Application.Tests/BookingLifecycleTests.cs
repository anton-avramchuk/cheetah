using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.DistributedLock;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Application.Bookings;
using Cheetah.Modules.Booking.Application.Exceptions;
using Cheetah.Modules.Booking.Domain.Slots;
using Cheetah.Modules.Booking.DomainEvents;
using Cheetah.Modules.Booking.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Booking.Application.Tests;

public class BookingLifecycleTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 5, 9, 0, 0, TimeSpan.Zero);

    private static Mock<IRepository<TestBooking, Guid>> RepoWith(TestBooking? booking, bool byToken = true)
    {
        var repo = new Mock<IRepository<TestBooking, Guid>>();
        if (byToken)
            repo.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<TestBooking>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
        else
            repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return repo;
    }

    [Fact]
    public async Task Cancel_ByToken_CancelsAndPublishes()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(), Start);
        var repo = RepoWith(booking);
        var bus = new FakeEventBus();

        await new CancelBookingCommandHandler<TestBooking>(repo.Object, bus)
            .HandleAsync(new CancelBookingCommand(booking.ManageToken, "changed plans", ByInvitee: true));

        booking.Status.ShouldBe(BookingStatus.Cancelled);
        bus.Published.OfType<BookingCancelledIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Cancel_UnknownToken_Throws()
    {
        var repo = RepoWith(null);
        await Should.ThrowAsync<BookingValidationException>(() =>
            new CancelBookingCommandHandler<TestBooking>(repo.Object, new FakeEventBus())
                .HandleAsync(new CancelBookingCommand("nope", "x", false)).AsTask());
    }

    [Fact]
    public async Task Reschedule_ByToken_MovesAndPublishes()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(durationMinutes: 60), Start);
        var repo = RepoWith(booking);
        var bus = new FakeEventBus();

        var calendar = new Mock<IBookingCalendarGateway>();
        calendar.Setup(c => c.GetBusyAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<BusyInterval>());
        var locks = new Mock<IDistributedLockProvider>();
        locks.Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FakeLock());

        var newStart = Start.AddDays(1);
        await new RescheduleBookingCommandHandler<TestBooking>(repo.Object, calendar.Object, locks.Object, bus)
            .HandleAsync(new RescheduleBookingCommand(booking.ManageToken, newStart));

        booking.Status.ShouldBe(BookingStatus.Rescheduled);
        booking.StartUtc.ShouldBe(newStart);
        booking.EndUtc.ShouldBe(newStart.AddMinutes(60));
        bus.Published.OfType<BookingRescheduledIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Reschedule_TargetBusy_ThrowsConflict()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(durationMinutes: 60), Start);
        var repo = RepoWith(booking);
        var newStart = Start.AddDays(1);

        var calendar = new Mock<IBookingCalendarGateway>();
        calendar.Setup(c => c.GetBusyAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new BusyInterval(newStart.AddMinutes(15), newStart.AddMinutes(45)) });
        var locks = new Mock<IDistributedLockProvider>();
        locks.Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FakeLock());

        await Should.ThrowAsync<BookingConflictException>(() =>
            new RescheduleBookingCommandHandler<TestBooking>(repo.Object, calendar.Object, locks.Object, new FakeEventBus())
                .HandleAsync(new RescheduleBookingCommand(booking.ManageToken, newStart)).AsTask());
    }

    [Fact]
    public async Task MarkNoShow_TransitionsAndPublishes()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(), Start);
        var repo = RepoWith(booking, byToken: false);
        var bus = new FakeEventBus();

        await new MarkNoShowCommandHandler<TestBooking>(repo.Object, bus)
            .HandleAsync(new MarkNoShowCommand(booking.Id));

        booking.Status.ShouldBe(BookingStatus.NoShow);
        bus.Published.OfType<BookingNoShowIntegrationEvent>().ShouldHaveSingleItem();
    }
}
