namespace Cheetah.Modules.Calendar.Application.Options;

/// <summary>Настройки конвейера напоминаний (секция <c>Calendar:Reminders</c>).</summary>
public sealed class CalendarReminderOptions
{
    /// <summary>На сколько дней вперёд материализуются срабатывания серий. Default = 60.</summary>
    public int HorizonDays { get; set; } = 60;

    /// <summary>Размер батча скана «пора слать». Default = 200.</summary>
    public int DispatchBatchSize { get; set; } = 200;

    /// <summary>Период скана «пора слать». Default = 1 минута.</summary>
    public TimeSpan DispatchPeriod { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Период фоновой материализации горизонта серий. Default = 1 час.</summary>
    public TimeSpan MaterializePeriod { get; set; } = TimeSpan.FromHours(1);

    /// <summary>Ключ distributed-lock для скана отправки (единичный исполнитель в кластере).</summary>
    public string DispatchLockKey { get; set; } = "calendar:reminders:dispatch";

    /// <summary>Ключ distributed-lock для материализации.</summary>
    public string MaterializeLockKey { get; set; } = "calendar:reminders:materialize";
}
