namespace Cheetah.Modules.Workflow.Shared;

/// <summary>Тип триггера правила.</summary>
public enum TriggerType
{
    /// <summary>Запуск по интеграционному событию из шины (firehose).</summary>
    Event = 0,
    /// <summary>Запуск по расписанию (cron).</summary>
    Schedule = 1
}

/// <summary>Статус срабатывания правила / отдельного шага.</summary>
public enum RunStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2,
    PartiallyFailed = 3
}

/// <summary>Поведение при сбое действия в правиле.</summary>
public enum ActionFailureMode
{
    /// <summary>Прервать исполнение оставшихся действий правила.</summary>
    StopRule = 0,
    /// <summary>Зафиксировать ошибку и продолжить со следующего действия.</summary>
    ContinueNext = 1,
    /// <summary>Откатить уже выполненные действия (через Cheetah.Saga).</summary>
    Compensate = 2
}
