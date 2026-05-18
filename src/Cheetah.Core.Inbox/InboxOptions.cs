namespace Cheetah.Core.Inbox;

/// <summary>
/// Параметры Inbox: сколько хранить записи и как часто чистить.
/// Биндится из конфигурации (секция "Inbox").
/// </summary>
public class InboxOptions
{
    /// <summary>
    /// Сколько хранить обработанные InboxMessages до удаления.
    /// </summary>
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Интервал запуска cleanup-сервиса. Default = 1 час.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Размер batch'а удаления (защита от блокировки большой таблицы).
    /// </summary>
    public int CleanupBatchSize { get; set; } = 1000;
}
