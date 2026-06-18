using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Contracts;

// ── Типы встреч (host) ──────────────────────────────────────────────────────────────────────

/// <summary>
/// Базовый запрос на создание типа встречи. Абстрактен: наследник объявляет конкретный
/// <c>sealed record CreateBookingTypeRequest : CreateBookingTypeRequestBase</c> и добавляет поля.
/// </summary>
public abstract record CreateBookingTypeRequestBase
{
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
}

/// <summary>Базовый запрос на обновление базовых полей типа встречи.</summary>
public abstract record UpdateBookingTypeRequestBase
{
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
}

// ── Доступность (host) ──────────────────────────────────────────────────────────────────────

/// <summary>Окно доступности в дне недели (локальное время host'а).</summary>
public sealed record WeeklyAvailabilityRuleDto(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

/// <summary>Исключение доступности на конкретную дату.</summary>
public sealed record AvailabilityDateOverrideDto(
    DateOnly Date, bool IsUnavailable, TimeOnly? StartTime, TimeOnly? EndTime);

/// <summary>Upsert недельной доступности host'а с исключениями.</summary>
public sealed record UpsertAvailabilityRequest(
    Guid HostUserId,
    string TimeZoneId,
    IReadOnlyList<WeeklyAvailabilityRuleDto> WeeklyRules,
    IReadOnlyList<AvailabilityDateOverrideDto> DateOverrides);

// ── Публичная запись (invitee) ──────────────────────────────────────────────────────────────

/// <summary>Ответ на intake-вопрос страницы записи.</summary>
public sealed record BookingAnswerDto(string Question, string? Value);

/// <summary>Создать бронь со страницы записи (публично, анонимно).</summary>
public sealed record CreatePublicBookingRequest(
    DateTimeOffset StartUtc,
    string InviteeName,
    string InviteeEmail,
    string? InviteePhone,
    string InviteeTimeZone,
    IReadOnlyList<BookingAnswerDto> Answers);

/// <summary>Перенести бронь по управляющему токену (invitee, без логина).</summary>
public sealed record RescheduleBookingRequest(DateTimeOffset NewStartUtc);

/// <summary>Отменить бронь по управляющему токену.</summary>
public sealed record CancelBookingRequest(string Reason);
