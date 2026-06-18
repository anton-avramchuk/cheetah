namespace Cheetah.Modules.Booking.Infrastructure.Calendar;

/// <summary>
/// Резолвит идентификатор календаря host'а, в который пишется итоговое событие брони. Calendar.Client
/// принимает <c>calendarId</c>, а у Booking есть только <c>hostUserId</c>, поэтому маппинг
/// host→календарь — точка конфигурации приложения. По умолчанию — <see cref="NullHostCalendarResolver"/>
/// (возвращает <c>null</c>, бронь создаётся без события Calendar). Приложение регистрирует свою
/// реализацию (например, поверх будущего Calendar-эндпоинта «календарь по владельцу»).
/// </summary>
public interface IHostCalendarResolver
{
    ValueTask<Guid?> ResolveAsync(Guid hostUserId, CancellationToken ct = default);
}

/// <summary>Резолвер по умолчанию: события Calendar для броней не создаются (бронь — источник истины).</summary>
public sealed class NullHostCalendarResolver : IHostCalendarResolver
{
    public ValueTask<Guid?> ResolveAsync(Guid hostUserId, CancellationToken ct = default)
        => ValueTask.FromResult<Guid?>(null);
}
