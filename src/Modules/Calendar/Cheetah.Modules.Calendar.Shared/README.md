# Cheetah.Modules.Calendar.Shared

Разделяемые enum'ы, константы и ключи Calendar. Зависит **только от `Cheetah.Core`** —
нижний слой, на который опираются Contracts, Domain, Infrastructure и Application.

## Состав

| Тип | Назначение |
|---|---|
| `CalendarType` | Тип календаря |
| `EventStatus` | `Confirmed` / `Cancelled` |
| `AttendeeRole` | `Organizer` / `Required` / `Optional` |
| `AttendeeResponse` | RSVP: `NeedsAction` / `Accepted` / `Declined` / `Tentative` |
| `ReminderTarget` | Кому слать напоминание: `Organizer` / `AcceptedAttendees` / `AllAttendees` |
| `ReminderTriggerStatus` | Статус материализованного срабатывания: `Pending` / `Sent` / `Skipped` / `Cancelled` |
| `CalendarConstants` | Имя схемы БД, ограничения длины полей |
| `CalendarTemplates`, `CalendarNotificationCategories` | Ключ шаблона напоминания и категория для `NotificationRequested` |

> Точные имена членов enum смотри в `CalendarEnums.cs`.

## Зависимости

`Cheetah.Core` — и больше ничего. Не ссылается на `Domain`/`Contracts`, чтобы оставаться
переиспользуемым нижним слоем модуля.
