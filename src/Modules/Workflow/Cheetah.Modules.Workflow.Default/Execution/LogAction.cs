using Cheetah.Core.DependencyInjection;
using Cheetah.Workflow;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.Workflow.Default.Execution;

/// <summary>
/// Встроенное действие «из коробки» без внешних зависимостей: пишет в лог. Демонстрирует plugin-точку
/// <see cref="IWorkflowAction"/>; реальные действия (CreateActivity, ChangeDealStage, …) контрибутят
/// соответствующие модули.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]
public sealed class LogAction : IWorkflowAction
{
    private readonly ILogger<LogAction> _logger;

    public LogAction(ILogger<LogAction> logger) => _logger = logger;

    public string Name => "Log";

    public ValueTask ExecuteAsync(WorkflowActionContext context, CancellationToken ct)
    {
        _logger.LogInformation("Workflow rule {RuleId} fired on {Event}: {@Parameters}",
            context.RuleId, context.Trigger.EventName, context.Parameters);
        return ValueTask.CompletedTask;
    }
}
