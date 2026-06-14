using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Client;

/// <summary>
/// HTTP-клиент к Calendar.Api для server-to-server интеграции: регистрация привязываемых типов
/// при старте, создание событий и чтение таймлайна сущности.
/// </summary>
public interface ICalendarClient
{
    /// <summary>Зарегистрировать (upsert) привязываемые типы сущностей сервиса. Идемпотентно.</summary>
    ValueTask SyncRegistryAsync(CalendarRegistrySyncRequest request, CancellationToken ct = default);

    /// <summary>Создать событие в указанном календаре.</summary>
    ValueTask<Guid> CreateEventAsync(Guid calendarId, CreateEventRequest request, CancellationToken ct = default);

    /// <summary>Получить экземпляры событий, привязанных к сущности, в окне [from, to).</summary>
    ValueTask<IReadOnlyList<EventOccurrenceDto>> GetByEntityAsync(
        string entityType, Guid entityId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
}
