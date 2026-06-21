using Cheetah.Core.DependencyInjection;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Infrastructure.Execution;

/// <summary>
/// Монолитный исполнитель действий: диспетчеризует по <see cref="IWorkflowAction.Name"/> среди
/// зарегистрированных in-proc плагинов. Микросервисные транспорты (Bus/Http) — follow-up (см. план §2.3).
/// </summary>
[Export(LifetimeType.Scoped, typeof(IWorkflowActionExecutor))]
public sealed class InProcActionExecutor : IWorkflowActionExecutor
{
    private readonly IReadOnlyDictionary<string, IWorkflowAction> _actions;

    public InProcActionExecutor(IEnumerable<IWorkflowAction> actions)
        => _actions = actions.ToDictionary(a => a.Name, StringComparer.Ordinal);

    public ValueTask ExecuteAsync(string actionType, WorkflowActionContext context, CancellationToken ct)
        => _actions.TryGetValue(actionType, out var action)
            ? action.ExecuteAsync(context, ct)
            : throw new InvalidOperationException($"No in-proc workflow action registered for '{actionType}'.");
}
