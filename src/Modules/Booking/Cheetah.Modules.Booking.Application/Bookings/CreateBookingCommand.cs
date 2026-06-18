using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.DistributedLock;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Application.Exceptions;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Domain.Specifications;

namespace Cheetah.Modules.Booking.Application.Bookings;

/// <summary>Создать бронь со страницы записи (slug) с защитой от двойной брони.</summary>
public sealed record CreateBookingCommand(string Slug, CreatePublicBookingRequest Request) : ICommand<Guid>;

public class CreateBookingCommandHandler<TBooking, TBookingType> : ICommandHandler<CreateBookingCommand, Guid>
    where TBooking : BookingBase
    where TBookingType : BookingTypeBase
{
    private readonly IRepository<TBooking, Guid> _bookings;
    private readonly IRepository<TBookingType, Guid> _types;
    private readonly IBookingFactory<TBooking, TBookingType> _factory;
    private readonly IBookingCalendarGateway _calendar;
    private readonly IDistributedLockProvider _locks;
    private readonly IBookingConfirmationHandler<TBooking> _confirmation;
    private readonly IEventBus _eventBus;

    public CreateBookingCommandHandler(
        IRepository<TBooking, Guid> bookings, IRepository<TBookingType, Guid> types,
        IBookingFactory<TBooking, TBookingType> factory, IBookingCalendarGateway calendar,
        IDistributedLockProvider locks, IBookingConfirmationHandler<TBooking> confirmation, IEventBus eventBus)
    {
        _bookings = bookings;
        _types = types;
        _factory = factory;
        _calendar = calendar;
        _locks = locks;
        _confirmation = confirmation;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateBookingCommand command, CancellationToken ct = default)
    {
        var type = await _types.GetBySpecAsync(new BookingTypeBySlugSpecification<TBookingType>(command.Slug), ct)
            ?? throw new BookingValidationException($"Booking page '{command.Slug}' not found");

        var startUtc = command.Request.StartUtc;
        var endUtc = startUtc.AddMinutes(type.DurationMinutes);

        // Анти-дабл-букинг: лок на конкретный слот host'а, затем повторная проверка занятости внутри лока.
        var lockKey = $"booking:{type.HostUserId}:{startUtc.UtcDateTime:O}";
        await using var handle = await _locks.TryAcquireAsync(lockKey, ct)
            ?? throw new BookingConflictException("Slot is being booked by someone else.");

        await EnsureSlotFreeAsync(type.HostUserId, startUtc, endUtc, ct);

        var booking = _factory.Create(type, startUtc, command.Request);
        _bookings.Add(booking);
        await _bookings.SaveChangesAsync(ct);

        // Создаём событие в календаре host'а; организатор — host, invitee — в описании.
        var eventId = await _calendar.CreateEventAsync(booking, type, ct);
        if (eventId is { } id)
        {
            booking.AttachCalendarEvent(id);
            await _bookings.SaveChangesAsync(ct);
        }

        await _confirmation.OnConfirmedAsync(booking, type, ct);

        foreach (var e in booking.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        booking.ClearDomainEvents();

        return booking.Id;
    }

    private async ValueTask EnsureSlotFreeAsync(Guid hostUserId, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct)
    {
        var busy = await _calendar.GetBusyAsync(hostUserId, startUtc, endUtc, ct);
        if (busy.Any(b => b.StartUtc < endUtc && b.EndUtc > startUtc))
            throw new BookingConflictException("Slot is no longer available.");

        var existing = await _bookings.GetAllAsync(
            new ActiveBookingsByHostInRangeSpecification<TBooking>(hostUserId, startUtc, endUtc), ct);
        if (existing.Count > 0)
            throw new BookingConflictException("Slot is already booked.");
    }
}
