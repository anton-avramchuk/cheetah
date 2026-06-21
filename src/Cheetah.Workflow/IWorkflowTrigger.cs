namespace Cheetah.Workflow;

/// <summary>
/// Plugin-триггер (опциональная ось расширения). Встроенные триггеры — событие из шины (firehose) и
/// расписание (cron). Пользовательский триггер активирует свой источник и «толкает» нормализованные
/// конверты в движок через <see cref="WorkflowTriggerSink"/>.
/// </summary>
public interface IWorkflowTrigger
{
    /// <summary>Имя триггера — совпадает с custom-значением <c>TriggerBinding.TriggerKey</c>.</summary>
    string Name { get; }

    ValueTask ActivateAsync(WorkflowTriggerSink sink, CancellationToken ct);
}

/// <summary>Канал, в который пользовательский триггер «толкает» конверты движку правил.</summary>
public sealed record WorkflowTriggerSink(Func<WorkflowEventEnvelope, CancellationToken, ValueTask> Push);
