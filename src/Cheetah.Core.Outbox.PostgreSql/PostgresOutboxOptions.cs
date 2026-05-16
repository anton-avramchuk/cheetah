namespace Cheetah.Core.Outbox.PostgreSql;

public class PostgresOutboxOptions
{
    /// <summary>
    /// Имя канала pg_notify. Должно совпадать с тем, что указано в триггере.
    /// </summary>
    public string ChannelName { get; set; } = "outbox_new";

    /// <summary>
    /// Строка подключения для отдельного коннекта-слушателя.
    /// Если null — берётся из ConnectionStrings секции по имени ConnectionStringName.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Имя секции в ConnectionStrings (если ConnectionString не задан).
    /// </summary>
    public string ConnectionStringName { get; set; } = "Outbox";

    /// <summary>
    /// Базовая задержка переподключения при разрыве LISTEN-коннекта.
    /// Растёт экспоненциально до MaxReconnectDelay.
    /// </summary>
    public TimeSpan BaseReconnectDelay { get; set; } = TimeSpan.FromSeconds(1);

    public TimeSpan MaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);
}
