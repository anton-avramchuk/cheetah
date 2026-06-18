using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Default.Contracts;
using Cheetah.Modules.Booking.Default.Entities;
using BookingEntity = Cheetah.Modules.Booking.Default.Entities.Booking;

namespace Cheetah.Modules.Booking.Default.Factories;

public sealed class BookingTypeFactory : IBookingTypeFactory<BookingType, CreateBookingTypeRequest>
{
    public BookingType Create(CreateBookingTypeRequest request) => BookingType.Create(request);
}

public sealed class ScheduleFactory : IScheduleFactory<AvailabilitySchedule>
{
    public AvailabilitySchedule Create(Guid hostUserId, string timeZoneId)
        => AvailabilitySchedule.Create(hostUserId, timeZoneId);
}

public sealed class BookingFactory : IBookingFactory<BookingEntity, BookingType>
{
    public BookingEntity Create(BookingType type, DateTimeOffset startUtc, CreatePublicBookingRequest request)
        => BookingEntity.Reserve(type, startUtc, request);
}

public sealed class BookingTypeProjector : IBookingTypeProjector<BookingType, BookingTypeDto>
{
    public BookingTypeDto ToDto(BookingType t) => new()
    {
        Id = t.Id,
        HostUserId = t.HostUserId,
        Slug = t.Slug,
        Name = t.Name,
        DurationMinutes = t.DurationMinutes,
        LocationKind = t.LocationKind,
        LocationDetails = t.LocationDetails,
        BufferBeforeMinutes = t.BufferBeforeMinutes,
        BufferAfterMinutes = t.BufferAfterMinutes,
        MinNoticeMinutes = t.MinNoticeMinutes,
        MaxAdvanceDays = t.MaxAdvanceDays,
        SlotStepMinutes = t.SlotStepMinutes,
        Color = t.Color,
        IsActive = t.IsActive,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}

public sealed class BookingProjector : IBookingProjector<BookingEntity, BookingDto>
{
    public BookingDto ToDto(BookingEntity b) => new()
    {
        Id = b.Id,
        BookingTypeId = b.BookingTypeId,
        HostUserId = b.HostUserId,
        InviteeName = b.InviteeName,
        InviteeEmail = b.InviteeEmail,
        InviteePhone = b.InviteePhone,
        InviteeTimeZone = b.InviteeTimeZone,
        StartUtc = b.StartUtc,
        EndUtc = b.EndUtc,
        Status = b.Status,
        CalendarEventId = b.CalendarEventId,
        CreatedLeadId = b.CreatedLeadId,
        CreatedActivityId = b.CreatedActivityId,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
