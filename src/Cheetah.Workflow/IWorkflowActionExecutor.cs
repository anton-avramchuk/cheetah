namespace Cheetah.Workflow;

/// <summary>
/// Порт-диспетчер действий. По <c>actionType</c> выбирает транспорт/реализацию и исполняет действие.
/// Движок правил всегда зовёт этот порт и НЕ зависит от выбранного транспорта (in-proc / шина / HTTP).
/// </summary>
public interface IWorkflowActionExecutor
{
    ValueTask ExecuteAsync(string actionType, WorkflowActionContext context, CancellationToken ct);
}
