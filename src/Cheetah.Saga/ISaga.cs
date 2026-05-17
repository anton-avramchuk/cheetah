using Cheetah.Core.Events;

namespace Cheetah.Saga;

/// <summary>
/// Не-дженерик контракт саги для DI/orchestrator. Прикладной код наследует Saga&lt;TData&gt;.
/// </summary>
public interface ISaga
{
    Type DataType { get; }
    SagaStatus Status { get; }
    string? Reason { get; }

    /// <summary>Текущие данные (object-обёртка над TData).</summary>
    object Data { get; set; }

    /// <summary>Извлекает CorrelationKey из события для поиска saga instance.</summary>
    string GetCorrelation(IEvent @event);

    /// <summary>Диспатч события на соответствующий On(EventType) метод саги.</summary>
    ValueTask HandleAsync(IEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Перевести в статус Failed. Используется orchestrator'ом при unhandled exception в handler'е.
    /// </summary>
    void Fail(string reason);
}
