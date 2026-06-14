using Cheetah.Modules.Calendar.Domain;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.DomainEvents;
using Cheetah.Modules.Calendar.Shared;
using Shouldly;

namespace Cheetah.Modules.Calendar.Domain.Tests;

public class CalendarEventTests
{
    private static readonly DateTime Start = new(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc);

    private static CalendarEvent New(Guid? organizer = null)
        => CalendarEvent.Schedule(Guid.NewGuid(), "Meeting", Start, Start.AddHours(1), organizer ?? Guid.NewGuid());

    [Fact]
    public void Schedule_AddsOrganizerAttendee_AndRaisesEvent()
    {
        var organizer = Guid.NewGuid();
        var e = New(organizer);

        e.Attendees.ShouldHaveSingleItem();
        e.Attendees.First().UserId.ShouldBe(organizer);
        e.Attendees.First().Role.ShouldBe(AttendeeRole.Organizer);
        e.Attendees.First().Response.ShouldBe(AttendeeResponse.Accepted);
        e.DomainEvents.OfType<CalendarEventScheduledEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Schedule_EndBeforeStart_Throws()
        => Should.Throw<ArgumentException>(() =>
            CalendarEvent.Schedule(Guid.NewGuid(), "X", Start, Start.AddHours(-1), Guid.NewGuid()));

    [Fact]
    public void AddAttendee_IsIdempotent_PerUser()
    {
        var e = New();
        var user = Guid.NewGuid();
        e.AddAttendee(user, AttendeeRole.Required);
        e.AddAttendee(user, AttendeeRole.Optional);

        e.Attendees.Count(a => a.UserId == user).ShouldBe(1);
    }

    [Fact]
    public void RespondToInvite_UnknownUser_Throws()
        => Should.Throw<InvalidOperationException>(() => New().RespondToInvite(Guid.NewGuid(), AttendeeResponse.Accepted));

    [Fact]
    public void Cancel_SetsStatusAndRemovedAt_AndRaisesEvent()
    {
        var e = New();
        e.Cancel();

        e.Status.ShouldBe(EventStatus.Cancelled);
        e.RemovedAt.ShouldNotBeNull();
        e.DomainEvents.OfType<CalendarEventCancelledEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Cancel_IsIdempotent()
    {
        var e = New();
        e.Cancel();
        e.ClearDomainEvents();
        e.Cancel();
        e.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void CancelOccurrence_AddsSingleOverride()
    {
        var e = New();
        var key = RecurrenceKeys.FromUtc(Start);
        e.CancelOccurrence(key);
        e.CancelOccurrence(key);
        e.Overrides.Count(o => o.OccurrenceKey == key && o.IsCancelled).ShouldBe(1);
    }

    [Fact]
    public void RecurrenceKeys_RoundTrips()
    {
        var key = RecurrenceKeys.FromUtc(Start);
        RecurrenceKeys.TryParse(key, out var parsed).ShouldBeTrue();
        parsed.ShouldBe(Start);
    }
}
