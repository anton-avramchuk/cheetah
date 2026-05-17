namespace Cheetah.Saga;

public enum SagaStatus : byte
{
    /// <summary>Сага активна, ждёт следующего события.</summary>
    Running = 1,

    /// <summary>Завершена успешно — saga.Complete().</summary>
    Completed = 2,

    /// <summary>Запущена компенсация — saga.Compensate(reason).</summary>
    Compensating = 3,

    /// <summary>Компенсация завершена.</summary>
    Compensated = 4,

    /// <summary>Завершена с ошибкой — последний handler бросил unhandled exception.</summary>
    Failed = 5
}
