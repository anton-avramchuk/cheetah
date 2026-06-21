using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Workflow.Application;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Infrastructure;

/// <summary>
/// Подписчик firehose: получает каждый <see cref="WorkflowEventEnvelope"/> с шины и отдаёт движку правил.
/// Один канал — весь поток интеграционных событий (см. план §2.2).
/// </summary>
[Export(LifetimeType.Scoped, typeof(IEventHandler<WorkflowEventEnvelope>))]
public sealed class WorkflowEnvelopeHandler : IEventHandler<WorkflowEventEnvelope>
{
    private readonly IWorkflowEngine _engine;

    public WorkflowEnvelopeHandler(IWorkflowEngine engine) => _engine = engine;

    public ValueTask HandleAsync(WorkflowEventEnvelope @event, CancellationToken cancellationToken = default)
        => _engine.HandleTriggerAsync(@event, cancellationToken);
}
