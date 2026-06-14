namespace Cheetah.Modules.Calendar.Shared;

/// <summary>Тип календаря — определяет его назначение и владельца.</summary>
public enum CalendarType
{
    /// <summary>Личный календарь пользователя.</summary>
    Personal,
    /// <summary>Календарь, привязанный к бизнес-сущности (сделка, проект и т.п.).</summary>
    Entity,
    /// <summary>Общий/командный календарь.</summary>
    Shared
}

/// <summary>Статус события.</summary>
public enum EventStatus
{
    /// <summary>Подтверждено.</summary>
    Confirmed,
    /// <summary>Предварительно (под вопросом).</summary>
    Tentative,
    /// <summary>Отменено.</summary>
    Cancelled
}

/// <summary>Роль участника события.</summary>
public enum AttendeeRole
{
    /// <summary>Организатор.</summary>
    Organizer,
    /// <summary>Обязательный участник.</summary>
    Required,
    /// <summary>Необязательный участник.</summary>
    Optional
}

/// <summary>Ответ участника на приглашение (RSVP).</summary>
public enum AttendeeResponse
{
    /// <summary>Ответ ещё не получен.</summary>
    NeedsAction,
    /// <summary>Принял.</summary>
    Accepted,
    /// <summary>Отклонил.</summary>
    Declined,
    /// <summary>Под вопросом.</summary>
    Tentative
}

/// <summary>Кого охватывает напоминание.</summary>
public enum ReminderTarget
{
    /// <summary>Только организатора.</summary>
    Organizer,
    /// <summary>Всех участников.</summary>
    AllAttendees,
    /// <summary>Только принявших приглашение.</summary>
    AcceptedAttendees
}

/// <summary>Статус материализованного срабатывания напоминания.</summary>
public enum ReminderTriggerStatus
{
    /// <summary>Ожидает отправки.</summary>
    Pending,
    /// <summary>Намерение уведомить опубликовано.</summary>
    Sent,
    /// <summary>Пропущено (событие отменено / экземпляр исключён).</summary>
    Skipped,
    /// <summary>Отменено при пересчёте серии.</summary>
    Cancelled
}
