# Cheetah.Modules.Booking — Scheduling / Booking (Calendly внутри CRM)

Публичные страницы записи: внешний invitee сам выбирает свободный слот у сотрудника (host) и бронирует
встречу. CRM-нативный аналог Calendly. Реализован как **расширяемый шаблон-модуль** (по образцу
[Activities](../Activities/README.md)/Customer): модуль поставляет абстрактные базовые типы + generic-хелперы,
а наследник дописывает свои `sealed`-типы. Готовая реализация «из коробки» — сборка `.Default`.

Полный план и решения — [`docs/modules/scheduling-booking.md`](../../../docs/modules/scheduling-booking.md).

## Состав сборок

| Сборка | Назначение |
|---|---|
| `*.DomainEvents` | `BookingConfirmed/Rescheduled/Cancelled/NoShow` (контракты событий) |
| `*.Shared` | `BookingStatus`/`LocationKind`, `BookingConstants`, `EntityRefKeys` (`crm.booking`) |
| `*.Contracts` | абстрактные `BookingDtoBase`/`BookingTypeDtoBase` + публичные `SlotDto`/`PublicBookingPageDto`, `Create/Update*RequestBase`, `CreatePublicBookingRequest` |
| `*.Domain` | абстрактные агрегаты `BookingTypeBase`/`AvailabilityScheduleBase`/`BookingBase` (+ дети), generic-спеки, **`ISlotEngine` + `DefaultSlotEngine`** |
| `*.Infrastructure` | `BookingDbContextBase`, EF-конфигурации (дети — owned), `AddBookingInfrastructure<>`, `BookingCalendarGateway` (поверх `Calendar.Client`), `IHostCalendarResolver` |
| `*.Application` | generic CQRS (типы встреч, доступность, слоты, брони), порты-фабрики/проекторы, `AddBookingApplication<>`, `StateMachine<BookingStatus>` |
| `*.Api` | `BookingEndpointsBase<>` (публичные анонимные + приватные host), `CheetahBookingApiModuleBase<>` |
| `*.Default` | sealed `Booking`/`BookingType`/`AvailabilitySchedule` + Contracts + `BookingDbContext` + миграция + единый модуль «из коробки» |
| `*.Client` | `IBookingClient` (server-to-server: чтение страницы/слотов, создание/управление бронью) |

## Ядро: слот-движок (`ISlotEngine`)

Чистая функция (юнит-тестируется без БД): **доступность ∩ горизонт ∩ min-notice − занятость − буферы**
→ свободные слоты в UTC. Учитывает таймзону расписания (IANA/Windows), исключения дат (`DateOverride`),
шаг сетки (`SlotStepMinutes`). Подменяема наследником через порт.

## Анти-дабл-букинг (две линии защиты)

1. **Приложение:** `CreateBookingCommand` берёт `IDistributedLockProvider` на ключ
   `booking:{host}:{slotUtc}`, внутри лока повторно проверяет занятость (Calendar free/busy + активные
   брони) и только потом создаёт бронь.
2. **БД:** частичный уникальный индекс `(HostUserId, StartUtc)` среди активных статусов
   (`Confirmed`/`Rescheduled`) — гонка исключается даже при сбое лока.

## ⚠️ Предпосылка и шов интеграции с Calendar

- **Free/busy.** Модуль берёт занятость host'а из Calendar (`ICalendarClient.GetUserBusyAsync`,
  реализован — см. [`docs/modules/calendar-free-busy.md`](../../../docs/modules/calendar-free-busy.md)).
- **Создание события.** `ICalendarClient.CreateEventAsync` требует `calendarId`, а Booking знает только
  `hostUserId`. Маппинг host→календарь вынесен в порт **`IHostCalendarResolver`**. По умолчанию —
  `NullHostCalendarResolver` (бронь создаётся без события Calendar; источник истины — сама бронь).
  Приложение регистрирует свою реализацию.

## Точки расширяемости

- сущности — `abstract BookingTypeBase`/`AvailabilityScheduleBase`/`BookingBase` + `protected InitializeCore(...)` + `virtual`-мутаторы;
- ViewModel/запросы — `abstract record *DtoBase`/`*RequestBase`;
- создание/проекция — `IBookingFactory`/`IBookingProjector`, `IBookingTypeFactory`/`IBookingTypeProjector`, `IScheduleFactory`;
- схема EF — `*ConfigurationBase<T>` + `ConfigureCustom`-hook;
- слот-движок — порт `ISlotEngine`;
- пост-подтверждение (создать Lead/Activity) — `IBookingConfirmationHandler` (дефолт — no-op);
- календарь host'а — `IHostCalendarResolver`;
- «быстрый» карман без миграций — колонка `Attributes (jsonb)` на типе встречи и брони.

## Использование «из коробки» (`.Default`)

```csharp
// bootstrap: подключить модуль Default (тянет Infrastructure/Application/Api по зависимостям)
// строка подключения БД — "Booking"; применить миграцию InitialBooking
modules.Add<CheetahBookingDefaultModule>();

// при необходимости — реальный резолвер календаря host'а вместо no-op
services.AddScoped<IHostCalendarResolver, MyHostCalendarResolver>();
```

## Своя реализация (расширение)

```csharp
public sealed class Booking : BookingBase { public string? UtmSource { get; private set; } /* + фабрика */ }
public sealed record BookingDto : BookingDtoBase { public string? UtmSource { get; init; } }
// + factory/projector, sealed config с ConfigureCustom, DbContext + миграция, регистрация:
services.AddBookingInfrastructure<MyDbContext, Booking, BookingType, Schedule>();
services.AddBookingApplication<BookingType, Booking, Schedule, /* Contracts */, /* factories/projectors */>();
```

## Эндпоинты

**Публичные (анонимные, за RateLimit-политикой приложения):**

| Метод | Маршрут |
|---|---|
| GET | `api/public/booking/{slug}` — описание страницы |
| GET | `api/public/booking/{slug}/slots?from=&to=&tz=` — свободные слоты |
| POST | `api/public/booking/{slug}` — создать бронь |
| POST | `api/public/booking/manage/{token}/reschedule` · `/cancel` — управление по токену |

**Приватные (host):** CRUD `api/booking-types`, `PUT api/availability`,
`GET api/bookings`, `GET api/bookings/{id}`, `POST api/bookings/{id}/no-show`.

## Статус

MVP реализован: Domain (слот-движок), Infrastructure (EF + анти-дабл-букинг + шлюз Calendar),
Application (слоты + бронь + lifecycle), Api, Default (+ миграция), Client. Тесты: Domain 17,
Application 13, Client 5 — зелёные.

**Follow-up:** подписка на `BookingCancelled`/`Rescheduled` → синхронизация события Calendar
(нужны методы отмены/переноса в `Calendar.Client`); реальный `IHostCalendarResolver`; Saga-компенсации;
round-robin (несколько host'ов); RateLimit-политика на публичных маршрутах; интеграционный smoke-тест `.Default`.
