namespace Cheetah.Workflow;

/// <summary>
/// Plugin-действие — главная (поведенческая) точка расширения Workflow. Реализуется модулем-контрибутором
/// (in-proc, через <c>[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]</c>) либо синтезируется из
/// дескриптора для удалённого транспорта. Имя должно совпадать с <c>RuleAction.ActionType</c> и
/// <see cref="ActionDescriptor.Name"/>.
/// </summary>
public interface IWorkflowAction
{
    /// <summary>Имя действия — совпадает с <c>RuleAction.ActionType</c> и <see cref="ActionDescriptor.Name"/>.</summary>
    string Name { get; }

    ValueTask ExecuteAsync(WorkflowActionContext context, CancellationToken ct);
}

/// <summary>Контекст исполнения действия: какое правило/запуск, исходное событие и отрендеренные параметры.</summary>
public sealed record WorkflowActionContext(
    Guid RuleId,
    Guid RunId,
    WorkflowEventEnvelope Trigger,
    IReadOnlyDictionary<string, object?> Parameters);
