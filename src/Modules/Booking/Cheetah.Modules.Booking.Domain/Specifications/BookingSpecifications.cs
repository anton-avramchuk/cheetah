using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Domain.Specifications;

/// <summary>Активный тип встречи по уникальному slug публичной страницы.</summary>
public sealed class BookingTypeBySlugSpecification<TBookingType> : Specification<TBookingType>
    where TBookingType : BookingTypeBase
{
    private readonly string _slug;

    public BookingTypeBySlugSpecification(string slug) => _slug = slug.Trim().ToLowerInvariant();

    public override Expression<Func<TBookingType, bool>> ToExpression()
        => t => t.Slug == _slug && t.IsActive && t.RemovedAt == null;
}

/// <summary>Расписание доступности конкретного host'а.</summary>
public sealed class ScheduleByHostSpecification<TSchedule> : Specification<TSchedule>
    where TSchedule : AvailabilityScheduleBase
{
    private readonly Guid _hostUserId;

    public ScheduleByHostSpecification(Guid hostUserId) => _hostUserId = hostUserId;

    public override Expression<Func<TSchedule, bool>> ToExpression()
        => s => s.HostUserId == _hostUserId;
}

/// <summary>
/// Активные брони host'а, пересекающие окно <c>[fromUtc, toUtc)</c> — для повторной проверки
/// занятости и уникальности слота при подтверждении.
/// </summary>
public sealed class ActiveBookingsByHostInRangeSpecification<TBooking> : Specification<TBooking>
    where TBooking : BookingBase
{
    private readonly Guid _hostUserId;
    private readonly DateTimeOffset _fromUtc;
    private readonly DateTimeOffset _toUtc;

    public ActiveBookingsByHostInRangeSpecification(Guid hostUserId, DateTimeOffset fromUtc, DateTimeOffset toUtc)
    {
        _hostUserId = hostUserId;
        _fromUtc = fromUtc;
        _toUtc = toUtc;
    }

    public override Expression<Func<TBooking, bool>> ToExpression()
        => b => b.HostUserId == _hostUserId
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Rescheduled)
                && b.StartUtc < _toUtc && b.EndUtc > _fromUtc;
}

/// <summary>Бронь по управляющему токену (для self-service переноса/отмены invitee).</summary>
public sealed class BookingByManageTokenSpecification<TBooking> : Specification<TBooking>
    where TBooking : BookingBase
{
    private readonly string _token;

    public BookingByManageTokenSpecification(string token) => _token = token;

    public override Expression<Func<TBooking, bool>> ToExpression()
        => b => b.ManageToken == _token;
}
