using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Workflow;
using Shouldly;

namespace Cheetah.Modules.Workflow.Client.Tests;

public class ExecuteActionRequestedHandlerTests
{
    private sealed class RecordingAction(string name) : IWorkflowAction
    {
        public string Name { get; } = name;
        public bool Executed { get; private set; }
        public ValueTask ExecuteAsync(WorkflowActionContext context, CancellationToken ct)
        {
            Executed = true;
            return ValueTask.CompletedTask;
        }
    }

    private static WorkflowEventEnvelope Envelope() =>
        new("DealWonIntegrationEvent", Guid.NewGuid(), new Dictionary<string, object?>());

    private static ExecuteActionRequestedIntegrationEvent Request(string actionType) =>
        new(Guid.NewGuid(), Guid.NewGuid(), actionType, new Dictionary<string, object?>(), Envelope());

    [Fact]
    public async Task Routes_to_local_action_by_name()
    {
        var action = new RecordingAction("CreateActivity");
        var handler = new ExecuteActionRequestedHandler([action]);

        await handler.HandleAsync(Request("CreateActivity"));

        action.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task Ignores_unknown_action()
    {
        var action = new RecordingAction("CreateActivity");
        var handler = new ExecuteActionRequestedHandler([action]);

        await handler.HandleAsync(Request("SomeОтherService.Action")); // не наше — игнор без ошибки

        action.Executed.ShouldBeFalse();
    }
}
