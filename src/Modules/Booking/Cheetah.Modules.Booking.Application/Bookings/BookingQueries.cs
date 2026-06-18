using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Domain.Specifications;
using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Application.Bookings;

// ── Бронь по Id ────────────────────────────────────────────────────────────────────────────

/// <summary>Получить бронь по идентификатору (null, если не найдена).</summary>
public sealed record GetBookingByIdQuery<TDto>(Guid Id) : IQuery<TDto?>
    where TDto : BookingDtoBase;

public class GetBookingByIdQueryHandler<TBooking, TDto> : IQueryHandler<GetBookingByIdQuery<TDto>, TDto?>
    where TBooking : BookingBase
    where TDto : BookingDtoBase
{
    private readonly IRepository<TBooking, Guid> _repository;
    private readonly IBookingProjector<TBooking, TDto> _projector;

    public GetBookingByIdQueryHandler(IRepository<TBooking, Guid> repository, IBookingProjector<TBooking, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetBookingByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var booking = await _repository.GetByIdAsync(query.Id, ct);
        return booking is null ? null : _projector.ToDto(booking);
    }
}

// ── Список броней (host) ───────────────────────────────────────────────────────────────────

/// <summary>Список броней с комбинированным фильтром (любой критерий опционален).</summary>
public sealed record ListBookingsQuery<TDto>(Guid? HostUserId, BookingStatus? Status, DateTimeOffset? From, DateTimeOffset? To)
    : IQuery<IReadOnlyList<TDto>>
    where TDto : BookingDtoBase;

public class ListBookingsQueryHandler<TBooking, TDto> : IQueryHandler<ListBookingsQuery<TDto>, IReadOnlyList<TDto>>
    where TBooking : BookingBase
    where TDto : BookingDtoBase
{
    private readonly IRepository<TBooking, Guid> _repository;
    private readonly IBookingProjector<TBooking, TDto> _projector;

    public ListBookingsQueryHandler(IRepository<TBooking, Guid> repository, IBookingProjector<TBooking, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListBookingsQuery<TDto> query, CancellationToken ct = default)
    {
        var spec = new BookingsFilterSpecification<TBooking>(query.HostUserId, query.Status, query.From, query.To);
        var items = await _repository.GetAllAsync(spec, ct);
        return items.Select(_projector.ToDto).ToArray();
    }
}
