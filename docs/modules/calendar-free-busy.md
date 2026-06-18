# Calendar free/busy (`GetUserBusyQuery`) — предпосылка для Booking

> Статус: **реализовано.** Код — в `src/Modules/Calendar/` (Contracts/Application/Api/Client).
> Тесты зелёные: Application 10 (7 новых на `GetUserBusyQueryHandler` + слияние интервалов),
> Client 5 (2 новых на `GetUserBusyAsync`). Это **follow-up модуля Calendar** и снятый
> **предусловие-блокер** для [Scheduling / Booking](./scheduling-booking.md) (§0.1): Booking вычитает
> занятость host'а из его доступности через `ICalendarClient.GetUserBusyAsync`.
>
> Источник: [plans.md §12.8 п.1](../plans.md).

---

## 1. Что уже есть в Calendar (переиспользуем почти всё)

Сверено по коду `src/Modules/Calendar/`:

| Что | Где | Готовность |
|---|---|---|
| События, где пользователь — участник, в окне `[from, to)` | `EventsByAttendeeInRangeSpecification(userId, from, to)` (`…Domain/Specifications/CalendarEventSpecifications.cs`) | ✅ есть |
| **Организатор всегда участник** (значит «занятость host'а» = его повестка) | `CalendarEvent.Schedule(...)` добавляет `EventAttendee(Organizer)` (`…Domain/Entities/CalendarEvent.cs:88`) | ✅ есть |
| Разворот RRULE/EXDATE/override в конкретные экземпляры | `IRecurrenceExpander.Expand(@event, fromUtc, toUtc)` → `Occurrence(StartUtc, EndUtc, OccurrenceKey, IsOverride)` (`…Domain/Abstractions/IRecurrenceExpander.cs`) | ✅ есть (реализация в Infrastructure) |
| Готовый запрос «повестка пользователя» с разворотом | `ListAgendaQuery(UserId, FromUtc, ToUtc)` + handler (`…Application/Events/OccurrenceQueries.cs:64`) | ✅ есть |
| Эндпоинт повестки | `AgendaEndpoint` → `GET api/agenda/{userId}` (`…Api/Endpoints/EventEndpoints.cs:74`) | ✅ есть |
| Загрузка событий с деталями по спецификации | `ICalendarEventRepository.ListWithDetailsAsync(spec, ct)` | ✅ есть |

**Вывод: `GetUserBusyQuery` ≈ `ListAgendaQuery`**, но вместо `EventOccurrenceDto` отдаёт
**слитые занятые интервалы** `BusyIntervalDto`. Разворот серий, фильтр отменённых, привязка attendee —
уже решены. Объём работы — небольшой.

### 1.1. Отличия free/busy от повестки (что добавляем)

1. **Свёртка пересекающихся интервалов** (merge): две встречи 10:00–10:30 и 10:15–11:00 → один
   занятый интервал 10:00–11:00. Повестка их не сливает — для слот-движка свёртка важна (меньше
   интервалов → быстрее вычитание).
2. **Минимальная форма ответа** — только `StartUtc`/`EndUtc`, без заголовков/локаций (free/busy не
   раскрывает приватные детали чужих встреч; для публичной страницы записи это и приватность тоже).
3. **(Опц.) фильтр «занятости»** — если позже появится флаг «показывать как свободное»
   (`ShowAs = Free/Busy`) или `EventStatus.Tentative`, исключать такие из занятости. Сейчас такого
   поля нет → **все не-`Cancelled` события считаются занятыми** (спецификация уже фильтрует
   `Cancelled`).

---

## 2. Контракт (Contracts)

В `Cheetah.Modules.Calendar.Contracts/Dtos.cs`:

```csharp
/// <summary>Занятый интервал пользователя (UTC), полученный после разворота серий и слияния пересечений.</summary>
public sealed record BusyIntervalDto(DateTime StartUtc, DateTime EndUtc) : ICrmResponse;
```

> Booking-сторона маппит это в свой `BusyInterval` (`readonly record struct`) при приёме (см.
> [scheduling-booking.md §4.6](./scheduling-booking.md)).

## 3. Запрос + handler (Application)

В `…Application/Events/OccurrenceQueries.cs` рядом с `ListAgendaQuery` (тот же паттерн DI:
`ICalendarEventRepository` + `IRecurrenceExpander`):

```csharp
public sealed record GetUserBusyQuery(Guid HostUserId, DateTime FromUtc, DateTime ToUtc)
    : IQuery<IReadOnlyList<BusyIntervalDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserBusyQuery, IReadOnlyList<BusyIntervalDto>>))]
public sealed class GetUserBusyQueryHandler
    : IQueryHandler<GetUserBusyQuery, IReadOnlyList<BusyIntervalDto>>
{
    private readonly ICalendarEventRepository _events;
    private readonly IRecurrenceExpander _expander;

    public GetUserBusyQueryHandler(ICalendarEventRepository events, IRecurrenceExpander expander)
    {
        _events = events;
        _expander = expander;
    }

    public async ValueTask<IReadOnlyList<BusyIntervalDto>> HandleAsync(
        GetUserBusyQuery query, CancellationToken ct = default)
    {
        // 1. События, где host — участник (включая организатора), потенциально попадающие в окно.
        var events = await _events.ListWithDetailsAsync(
            new EventsByAttendeeInRangeSpecification(query.HostUserId, query.FromUtc, query.ToUtc), ct);

        // 2. Разворачиваем серии экспандером → плоский список интервалов в окне.
        var raw = events
            .SelectMany(e => _expander.Expand(e, query.FromUtc, query.ToUtc))
            .Select(o => (o.StartUtc, o.EndUtc))
            .Where(i => i.EndUtc > query.FromUtc && i.StartUtc < query.ToUtc);

        // 3. Сливаем пересекающиеся/смежные интервалы (см. §3.1).
        return BusyIntervalMerger.Merge(raw, query.FromUtc, query.ToUtc);
    }
}
```

### 3.1. Свёртка интервалов (чистая функция — юнит-тестируется)

Вынести в `internal static class BusyIntervalMerger` рядом с `OccurrenceExpansion`:

```
Merge(intervals, clipFrom, clipTo):
  sorted = intervals.OrderBy(Start)
  result = []
  foreach i in sorted:
     s = max(i.Start, clipFrom);  e = min(i.End, clipTo)   // обрезаем по окну
     if e <= s: continue
     if result пуст или s > result.Last.End:  result.add({s, e})
     else: result.Last.End = max(result.Last.End, e)        // расширяем текущий
  return result.Select(x => new BusyIntervalDto(x.s, x.e))
```

> Чистая функция без БД — основная цель тестов §5. Обрезка по `[from, to)` гарантирует, что слот-движок
> Booking получает интервалы строго в запрошенном окне.

## 4. Эндпоинт (Api) + клиент (Client)

**Эндпоинт** — рядом с `AgendaEndpoint` в `…Api/Endpoints/EventEndpoints.cs`, тем же декларативным
стилем (`QueryCollectionEndpoint`), + `Request`→`Query` маппинг в `CalendarMappingProfile`:

```csharp
/// <summary>GET api/calendar/users/{hostUserId}/busy?from=&to= — занятые интервалы пользователя (free/busy).</summary>
public sealed class GetUserBusyEndpoint
    : QueryCollectionEndpoint<GetUserBusyRequest, GetUserBusyQuery, BusyIntervalDto, BusyIntervalDto>
{
    public override string Route => "api/calendar/users/{hostUserId:guid}/busy";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetUserBusy").WithTags("Events");
}
```

`GetUserBusyRequest(Guid HostUserId, DateTime FromUtc, DateTime ToUtc)` — в `…Contracts/Requests.cs`
(по образцу `ListAgendaRequest`). Авторизация — server-to-server политика (как у остальных
read-эндпоинтов Calendar); публичную страницу записи Booking сам проксирует, наружу этот эндпоинт не
выставляется.

**Клиент** — добавить метод в `ICalendarClient` и `HttpCalendarClient`
(`…Client/ICalendarClient.cs`, `HttpCalendarClient.cs`), по образцу `GetByEntityAsync`:

```csharp
// ICalendarClient
ValueTask<IReadOnlyList<BusyIntervalDto>> GetUserBusyAsync(
    Guid hostUserId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct = default);

// HttpCalendarClient
public async ValueTask<IReadOnlyList<BusyIntervalDto>> GetUserBusyAsync(
    Guid hostUserId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct = default)
{
    var url = $"api/calendar/users/{hostUserId}/busy"
              + $"?from={Uri.EscapeDataString(fromUtc.UtcDateTime.ToString("O"))}"
              + $"&to={Uri.EscapeDataString(toUtc.UtcDateTime.ToString("O"))}";
    var resp = await _http.GetAsync(url, ct);
    await EnsureSuccessOrThrowAsync(resp, "GET /calendar/users/{id}/busy", ct);
    var body = await resp.Content.ReadFromJsonAsync<List<BusyIntervalDto>>(ct);
    return body ?? new List<BusyIntervalDto>();
}
```

## 5. Тесты

| Проект | Покрытие |
|---|---|
| `Application.Tests` | `GetUserBusyQueryHandler`: разовое событие → один интервал; серия RRULE → несколько; `Cancelled` исключены; экземпляр, отменённый override-ом, не попадает; обрезка по окну `[from, to)` |
| `Application.Tests` (`BusyIntervalMerger`) | пустой вход → пусто; непересекающиеся сохраняются; пересекающиеся сливаются; смежные (`end == start`) сливаются; вложенный поглощается; обрезка краёв окна |
| `Client.Tests` | `GetUserBusyAsync`: корректный URL с `from/to` в ISO-8601 UTC; десериализация; обработка не-2xx (`HttpRequestException`) |

## 6. Шаги (каждый = коммит)

1. **Contracts:** `BusyIntervalDto` (`Dtos.cs`), `GetUserBusyRequest` (`Requests.cs`).
2. **Application:** `GetUserBusyQuery` + handler + `BusyIntervalMerger` (`OccurrenceQueries.cs`);
   `Application.Tests` (handler + merger).
3. **Api:** `GetUserBusyEndpoint` + маппинг `GetUserBusyRequest→GetUserBusyQuery` в
   `CalendarMappingProfile`; обновить `…Api/README.md` (новая строка таблицы эндпоинтов).
4. **Client:** метод в `ICalendarClient` + `HttpCalendarClient`; `Client.Tests`; обновить
   README Calendar (Client — базовый модуль, README по чек-листу `CLAUDE.md`).
5. `dotnet build` + тесты зелёные → разблокирована Фаза 0 плана Booking
   ([scheduling-booking.md §11](./scheduling-booking.md)).

## 7. Заметки / открытые вопросы

- **Поведение при недоступности Calendar** (Booking-сторона): fail-closed — не показывать слоты /
  отклонять бронь, чем показать занятый слот свободным (двойная бронь хуже пустого календаря). Решение
  фиксируется в Booking ([scheduling-booking.md §1.2 п.1](./scheduling-booking.md)).
- **`DateTime` vs `DateTimeOffset`.** Calendar внутри оперирует `DateTime` (Kind=Utc) — сохраняем эту
  конвенцию в `BusyIntervalDto`/запросе; Booking конвертирует на своей границе.
- **Производительность.** Окно free/busy ограничено горизонтом `BookingType.MaxAdvanceDays` (обычно
  ≤ 60 дней) — разворот серий в таком окне дёшев; индекс под `EventsByAttendeeInRangeSpecification`
  уже работает на повестку.
- **Будущее (вне MVP):** флаг `ShowAs=Free/Busy` или учёт `Tentative` как «не блокирует»; внешние
  free-busy (Google/Outlook) — отдельный коннектор.
