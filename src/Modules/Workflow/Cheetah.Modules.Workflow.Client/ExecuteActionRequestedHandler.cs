using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Client;

/// <summary>
/// Микросервисный приёмник действий: в сервисе-цели подписывается на
/// <see cref="ExecuteActionRequestedIntegrationEvent"/> и маршрутизирует на локальный
/// <see cref="IWorkflowAction"/> по <c>ActionType</c>. Чужие действия игнорирует (их подберёт другой
/// сервис). Так Workflow-сервис исполняет действия, не завися от кода цели.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IEventHandler<ExecuteActionRequestedIntegrationEvent>))]
public sealed class ExecuteActionRequestedHandler : IEventHandler<ExecuteActionRequestedIntegrationEvent>
{
    private readonly IReadOnlyDictionary<string, IWorkflowAction> _local;

    public ExecuteActionRequestedHandler(IEnumerable<IWorkflowAction> actions)
        => _local = actions.ToDictionary(a => a.Name, StringComparer.Ordinal);

    public ValueTask HandleAsync(ExecuteActionRequestedIntegrationEvent @event, CancellationToken cancellationToken = default)
        => _local.TryGetValue(@event.ActionType, out var action)
            ? action.ExecuteAsync(
                new WorkflowActionContext(@event.RuleId, @event.RunId, @event.Trigger, @event.Parameters),
                cancellationToken)
            : ValueTask.CompletedTask;
}
