using Cheetah.Contracts.Responses;
using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Contracts;

/// <summary>
/// Базовый ViewModel брони (граница API). Абстрактен: наследник объявляет конкретный
/// <c>sealed record BookingDto : BookingDtoBase</c> и при необходимости добавляет свои поля.
/// Это точка расширяемости ViewModel.
/// </summary>
public abstract record BookingDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public Guid BookingTypeId { get; init; }
    public Guid HostUserId { get; init; }
    public string InviteeName { get; init; } = null!;
    public string InviteeEmail { get; init; } = null!;
    public string? InviteePhone { get; init; }
    public string InviteeTimeZone { get; init; } = null!;
    public DateTimeOffset StartUtc { get; init; }
    public DateTimeOffset EndUtc { get; init; }
    public BookingStatus Status { get; init; }
    public Guid? CalendarEventId { get; init; }
    public Guid? CreatedLeadId { get; init; }
    public Guid? CreatedActivityId { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>
/// Базовый ViewModel типа встречи (публичной страницы записи). Абстрактен: наследник объявляет
/// <c>sealed record BookingTypeDto : BookingTypeDtoBase</c> и добавляет свои поля.
/// </summary>
public abstract record BookingTypeDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public Guid HostUserId { get; init; }
    public string Slug { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int DurationMinutes { get; init; }
    public LocationKind LocationKind { get; init; }
    public string? LocationDetails { get; init; }
    public int BufferBeforeMinutes { get; init; }
    public int BufferAfterMinutes { get; init; }
    public int MinNoticeMinutes { get; init; }
    public int MaxAdvanceDays { get; init; }
    public int? SlotStepMinutes { get; init; }
    public string? Color { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>Свободный слот (UTC). Конкретный публичный DTO — расширять нечего.</summary>
public sealed record SlotDto(DateTimeOffset StartUtc, DateTimeOffset EndUtc, string InviteeLocalTime) : ICrmResponse;

/// <summary>Публичное описание страницы записи (то, что видит внешний invitee).</summary>
public sealed record PublicBookingPageDto(
    string Slug,
    string Name,
    int DurationMinutes,
    LocationKind LocationKind,
    string? LocationDetails) : ICrmResponse;
