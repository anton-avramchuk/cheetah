# Cheetah.Modules.Calendar.DomainEvents

Доменные события Calendar — чистые контракты (`EventBase`) для интеграции других модулей.
**Без зависимостей**, кроме `Cheetah.Core.Events`: привязка к сущности передаётся парой
`string?`/`Guid?`, роли/ответы — строками (не зависит от `Shared`). Другие модули подписываются
только на эту сборку (правило «depend only on Events»).

## Состав

| Событие | Когда публикуется |
|---|---|
| `CalendarEventScheduledEvent` | Событие запланировано (создано) |
| `CalendarEventRescheduledEvent` | Изменён период/таймзона |
| `CalendarEventDetailsChangedEvent` | Изменены заголовок/описание/локация |
| `CalendarEventRecurrenceChangedEvent` | Установлено/изменено правило повторения |
| `CalendarEventCancelledEvent` | Событие отменено |
| `EventAttendeeInvitedEvent` | Участник приглашён |
| `EventAttendeeRespondedEvent` | Участник ответил на приглашение (RSVP) |

Все публикуются **после** `SaveChangesAsync` (через Outbox того же `CalendarDbContext`).

## Подписка

```csharp
[DependsOn(typeof(CheetahCalendarDomainEventsModule))]
public class MyModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var bus = context.ServiceProvider.GetRequiredService<IEventBus>();
        bus.Subscribe<CalendarEventScheduledEvent, MyHandler>();
    }
}
```
