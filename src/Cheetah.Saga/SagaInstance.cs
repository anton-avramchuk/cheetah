namespace Cheetah.Saga;

/// <summary>
/// Персистентное состояние одного запущенного экземпляра саги.
/// Уникальный ключ — (SagaType, CorrelationKey).
/// </summary>
public class SagaInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FullName класса саги (например "MyApp.Sagas.OrderProcessingSaga").</summary>
    public string SagaType { get; set; } = null!;

    /// <summary>Ключ корреляции — обычно строковое представление aggregateId (OrderId).</summary>
    public string CorrelationKey { get; set; } = null!;

    public SagaStatus Status { get; set; } = SagaStatus.Running;

    /// <summary>Сериализованное TData. AssemblyQualifiedName типа лежит в DataType.</summary>
    public string DataJson { get; set; } = "{}";
    public string DataType { get; set; } = null!;

    /// <summary>Причина перехода в Compensating/Failed (для диагностики).</summary>
    public string? Reason { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Оптимистичная блокировка для предотвращения lost-update между репликами.</summary>
    public int Version { get; set; }
}
