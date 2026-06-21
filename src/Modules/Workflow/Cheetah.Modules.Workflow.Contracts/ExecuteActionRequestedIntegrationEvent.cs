using Cheetah.Core.Events;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Contracts;

/// <summary>
/// Команда-событие для микросервисного исполнения действия: Workflow-сервис публикует её, а целевой
/// сервис подбирает у себя по <see cref="ActionType"/> и исполняет своим <c>IWorkflowAction</c>.
/// Так Workflow исполняет действия в чужих модулях, не завися от их кода.
/// </summary>
public sealed record ExecuteActionRequestedIntegrationEvent(
    Guid RuleId,
    Guid RunId,
    string ActionType,
    IReadOnlyDictionary<string, object?> Parameters,
    WorkflowEventEnvelope Trigger) : EventBase;
