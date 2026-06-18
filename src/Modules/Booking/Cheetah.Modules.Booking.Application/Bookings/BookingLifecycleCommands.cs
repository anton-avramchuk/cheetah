using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.DistributedLock;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Application.Exceptions;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Domain.Specifications;

namespace Cheetah.Modules.Booking.Application.Bookings;

internal static class BookingPublisher
{
    public static async ValueTask SaveAndPublishAsync<TBooking>(
        IRepository<TBooking, Guid> repository, IEventBus eventBus, TBooking booking, CancellationToken ct)
        where TBooking : BookingBase
    {
        await repository.SaveChangesAsync(ct);
        foreach (var e in booking.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        booking.ClearDomainEvents();
    }

    public static async ValueTask<TBooking> ByTokenOrThrowAsync<TBooking>(
        IRepository<TBooking, Guid> repository, string token, CancellationToken ct)
        where TBooking : BookingBase
        => await repository.GetBySpecAsync(new BookingByManageTokenSpecification<TBooking>(token), ct)
            ?? throw new BookingValidationException("Booking not found for the provided token.");
}

// ── Перенос брони (self-service по токену) ──────────────────────────────────────────────────

/// <summary>Перенести бронь на новое время по управляющему токену invitee.</summary>
public sealed record RescheduleBookingCommand(string ManageToken, DateTimeOffset NewStartUtc) : ICommand;

public class RescheduleBookingCommandHandler<TBooking> : ICommandHandler<RescheduleBookingCommand>
    where TBooking : BookingBase
{
    private readonly IRepository<TBooking, Guid> _bookings;
    private readonly IBookingCalendarGateway _calendar;
    private readonly IDistributedLockProvider _locks;
    private readonly IEventBus _eventBus;

    public RescheduleBookingCommandHandler(
        IRepository<TBooking, Guid> bookings, IBookingCalendarGateway calendar,
        IDistributedLockProvider locks, IEventBus eventBus)
    {
        _bookings = bookings;
        _calendar = calendar;
        _locks = locks;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(RescheduleBookingCommand command, CancellationToken ct = default)
    {
        var booking = await BookingPublisher.ByTokenOrThrowAsync(_bookings, command.ManageToken, ct);

        var durationMinutes = (int)(booking.EndUtc - booking.StartUtc).TotalMinutes;
        var newStart = command.NewStartUtc;
        var newEnd = newStart.AddMinutes(durationMinutes);

        var lockKey = $"booking:{booking.HostUserId}:{newStart.UtcDateTime:O}";
        await using var handle = await _locks.TryAcquireAsync(lockKey, ct)
            ?? throw new BookingConflictException("Slot is being booked by someone else.");

        var busy = await _calendar.GetBusyAsync(booking.HostUserId, newStart, newEnd, ct);
        if (busy.Any(b => b.StartUtc < newEnd && b.EndUtc > newStart))
            throw new BookingConflictException("Target slot is no longer available.");

        booking.Reschedule(newStart, durationMinutes);
        await BookingPublisher.SaveAndPublishAsync(_bookings, _eventBus, booking, ct);
    }
}

// ── Отмена брони (по токену) ────────────────────────────────────────────────────────────────

/// <summary>Отменить бронь по управляющему токену.</summary>
public sealed record CancelBookingCommand(string ManageToken, string Reason, bool ByInvitee) : ICommand;

public class CancelBookingCommandHandler<TBooking> : ICommandHandler<CancelBookingCommand>
    where TBooking : BookingBase
{
    private readonly IRepository<TBooking, Guid> _bookings;
    private readonly IEventBus _eventBus;

    public CancelBookingCommandHandler(IRepository<TBooking, Guid> bookings, IEventBus eventBus)
    {
        _bookings = bookings;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(CancelBookingCommand command, CancellationToken ct = default)
    {
        var booking = await BookingPublisher.ByTokenOrThrowAsync(_bookings, command.ManageToken, ct);
        booking.Cancel(command.Reason, command.ByInvitee);
        await BookingPublisher.SaveAndPublishAsync(_bookings, _eventBus, booking, ct);
    }
}

// ── No-show (host) ──────────────────────────────────────────────────────────────────────────

/// <summary>Отметить бронь как «не явился» (host).</summary>
public sealed record MarkNoShowCommand(Guid BookingId) : ICommand;

public class MarkNoShowCommandHandler<TBooking> : ICommandHandler<MarkNoShowCommand>
    where TBooking : BookingBase
{
    private readonly IRepository<TBooking, Guid> _bookings;
    private readonly IEventBus _eventBus;

    public MarkNoShowCommandHandler(IRepository<TBooking, Guid> bookings, IEventBus eventBus)
    {
        _bookings = bookings;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(MarkNoShowCommand command, CancellationToken ct = default)
    {
        var booking = await _bookings.GetByIdAsync(command.BookingId, ct)
            ?? throw new BookingValidationException($"Booking '{command.BookingId}' not found");
        booking.MarkNoShow();
        await BookingPublisher.SaveAndPublishAsync(_bookings, _eventBus, booking, ct);
    }
}
