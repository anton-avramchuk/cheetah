using Cheetah.Modules.Booking.DomainEvents;
using Cheetah.Modules.Booking.Shared;
using Shouldly;

namespace Cheetah.Modules.Booking.Domain.Tests;

public class BookingBaseTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 5, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Reserve_SetsConfirmedAndDerivesEndFromDuration()
    {
        var type = TestBookingType.New(durationMinutes: 30);
        var booking = TestBooking.Reserve(type, Start);

        booking.Status.ShouldBe(BookingStatus.Confirmed);
        booking.EndUtc.ShouldBe(Start.AddMinutes(30));
        booking.HostUserId.ShouldBe(type.HostUserId);
        booking.IsActive.ShouldBeTrue();
        booking.ManageToken.ShouldNotBeNullOrWhiteSpace();
        booking.DomainEvents.OfType<BookingConfirmedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Reschedule_OnActive_MovesAndRaisesEvent()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(durationMinutes: 60), Start);
        var newStart = Start.AddDays(1);

        booking.Reschedule(newStart, 60);

        booking.Status.ShouldBe(BookingStatus.Rescheduled);
        booking.StartUtc.ShouldBe(newStart);
        booking.EndUtc.ShouldBe(newStart.AddMinutes(60));
        booking.DomainEvents.OfType<BookingRescheduledIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Cancel_OnActive_RaisesEvent()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(), Start);

        booking.Cancel("changed plans", byInvitee: true);

        booking.Status.ShouldBe(BookingStatus.Cancelled);
        booking.CancelReason.ShouldBe("changed plans");
        booking.IsActive.ShouldBeFalse();
        booking.DomainEvents.OfType<BookingCancelledIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Cancel_OnTerminal_Throws()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(), Start);
        booking.Cancel("first", byInvitee: false);

        Should.Throw<InvalidOperationException>(() => booking.Cancel("again", byInvitee: false));
    }

    [Fact]
    public void Reschedule_OnTerminal_Throws()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(), Start);
        booking.Cancel("x", byInvitee: false);

        Should.Throw<InvalidOperationException>(() => booking.Reschedule(Start.AddDays(1), 60));
    }

    [Fact]
    public void MarkNoShow_FromConfirmed_TransitionsAndRaisesEvent()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(), Start);

        booking.MarkNoShow();

        booking.Status.ShouldBe(BookingStatus.NoShow);
        booking.DomainEvents.OfType<BookingNoShowIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void MarkNoShow_FromCancelled_Ignored()
    {
        var booking = TestBooking.Reserve(TestBookingType.New(), Start);
        booking.Cancel("x", byInvitee: false);

        booking.MarkNoShow();

        booking.Status.ShouldBe(BookingStatus.Cancelled);
    }
}
