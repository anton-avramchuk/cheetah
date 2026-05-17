using Cheetah.Core.Events;

namespace Cheetah.Saga;

/// <summary>
/// Принимает событие и маршрутизирует его всем зарегистрированным сагам,
/// которые знают этот тип события. Для каждой саги:
/// - Резолвит CorrelationKey
/// - Загружает существующую SagaInstance или создаёт новую (если событие [SagaStartedBy])
/// - Десериализует данные, диспатчит On(event), сериализует обратно
/// - Сохраняет с оптимистичной блокировкой
/// </summary>
public interface ISagaOrchestrator
{
    ValueTask HandleAsync(IEvent @event, CancellationToken cancellationToken = default);
}
