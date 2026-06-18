using Cheetah.Modules.Booking.Contracts;

namespace Cheetah.Modules.Booking.Client;

/// <summary>
/// HTTP-клиент к Booking.Api для server-to-server интеграции: чтение публичной страницы записи и
/// свободных слотов, программное создание брони и управление ею по токену.
/// </summary>
public interface IBookingClient
{
    /// <summary>Публичное описание страницы записи по slug (null, если нет/неактивна).</summary>
    ValueTask<PublicBookingPageDto?> GetPageAsync(string slug, CancellationToken ct = default);

    /// <summary>Свободные слоты страницы записи в окне дат, в таймзоне invitee.</summary>
    ValueTask<IReadOnlyList<SlotDto>> GetSlotsAsync(
        string slug, DateOnly from, DateOnly to, string inviteeTimeZone, CancellationToken ct = default);

    /// <summary>Создать бронь. Возвращает Id брони.</summary>
    ValueTask<Guid> CreateBookingAsync(string slug, CreatePublicBookingRequest request, CancellationToken ct = default);

    /// <summary>Перенести бронь по управляющему токену.</summary>
    ValueTask RescheduleAsync(string manageToken, DateTimeOffset newStartUtc, CancellationToken ct = default);

    /// <summary>Отменить бронь по управляющему токену.</summary>
    ValueTask CancelAsync(string manageToken, string reason, CancellationToken ct = default);
}
