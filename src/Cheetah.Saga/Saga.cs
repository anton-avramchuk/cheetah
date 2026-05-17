using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Cheetah.Core.Events;

namespace Cheetah.Saga;

/// <summary>
/// Базовый класс саги. Наследник определяет:
///   1) тип данных TData (произвольный record/class с public свойствами);
///   2) методы вида ValueTask On(SomeEvent e, CancellationToken ct);
///   3) override GetCorrelation(IEvent) — как получить CorrelationKey из события;
///   4) атрибуты [SagaStartedBy(typeof(StartEvent))] / [SagaHandles(typeof(NextEvent))].
///
/// Lifecycle:
///   Running → Complete() → Completed
///   Running → Compensate(reason) → Compensating  (далее handlers продолжают идти, дают шанс отыграть)
///   Compensating → Complete() → Compensated
///   Любая unhandled exception в On(...) → Failed (orchestrator перехватывает).
/// </summary>
public abstract class Saga<TData> : ISaga where TData : class, new()
{
    private static readonly ConcurrentDictionary<(Type SagaType, Type EventType), MethodInfo?> HandlerCache = new();

    public TData Data { get; set; } = new();
    public SagaStatus Status { get; private set; } = SagaStatus.Running;
    public string? Reason { get; private set; }

    Type ISaga.DataType => typeof(TData);
    object ISaga.Data
    {
        get => Data;
        set => Data = (TData)value;
    }

    /// <summary>
    /// Должен возвращать ключ корреляции — обычно строковое представление aggregateId.
    /// Этот ключ должен быть одинаковым для всех событий одной саги-инстанции.
    /// </summary>
    public abstract string GetCorrelation(IEvent @event);

    public void Complete()
    {
        Status = Status == SagaStatus.Compensating ? SagaStatus.Compensated : SagaStatus.Completed;
    }

    public void Compensate(string reason)
    {
        Status = SagaStatus.Compensating;
        Reason = reason;
    }

    public void Fail(string reason)
    {
        Status = SagaStatus.Failed;
        Reason = reason;
    }

    public ValueTask HandleAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        var eventType = @event.GetType();
        var method = GetHandlerMethod(eventType);
        if (method is null)
        {
            // Не реализован handler для этого типа — мы попали сюда из-за [SagaHandles] на классе.
            // Не падаем — некоторые саги декларируют события "к сведению", не реагируют.
            return ValueTask.CompletedTask;
        }

        object? result;
        try
        {
            result = method.Invoke(this, new object[] { @event, cancellationToken });
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            // Распаковываем reflection-обёртку, чтобы orchestrator увидел оригинальный exception.
            ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
            throw; // never reached
        }
        return result switch
        {
            ValueTask vt => vt,
            Task t => new ValueTask(t),
            _ => ValueTask.CompletedTask
        };
    }

    private MethodInfo? GetHandlerMethod(Type eventType)
    {
        return HandlerCache.GetOrAdd((GetType(), eventType), key =>
        {
            // Ищем public ValueTask|Task On(EventType, CancellationToken) или просто On(EventType).
            foreach (var m in key.SagaType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (m.Name != "On") continue;
                var parameters = m.GetParameters();
                if (parameters.Length == 0) continue;
                if (parameters[0].ParameterType != key.EventType) continue;
                return m;
            }
            return null;
        });
    }
}
