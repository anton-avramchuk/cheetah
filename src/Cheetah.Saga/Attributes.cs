namespace Cheetah.Saga;

/// <summary>
/// Указывает, что событие может СТАРТОВАТЬ новую сагу этого типа.
/// При получении события, для которого нет существующей saga instance с таким CorrelationKey —
/// создаётся новая instance.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SagaStartedByAttribute : Attribute
{
    public Type EventType { get; }
    public SagaStartedByAttribute(Type eventType) => EventType = eventType;
}

/// <summary>
/// Указывает, что событие обрабатывается уже существующей сагой.
/// Если saga instance с таким CorrelationKey не найдена — событие игнорируется
/// (или логируется warning — в зависимости от настроек).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SagaHandlesAttribute : Attribute
{
    public Type EventType { get; }
    public SagaHandlesAttribute(Type eventType) => EventType = eventType;
}
