using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Deals.Application.Deals;
using Cheetah.Workflow;

namespace Cheetah.Modules.Deals.Workflow.Actions;

/// <summary>Действие Workflow: переместить сделку на стадию. Параметры: <c>dealId</c>, <c>toStageId</c>, <c>changedBy</c>.</summary>
[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]
public sealed class ChangeDealStageAction : IWorkflowAction
{
    private readonly IDispatcher _dispatcher;

    public ChangeDealStageAction(IDispatcher dispatcher) => _dispatcher = dispatcher;

    public string Name => DealsActions.ChangeStage;

    public ValueTask ExecuteAsync(WorkflowActionContext context, CancellationToken ct)
    {
        var dealId = context.GetGuid("dealId") ?? throw Required("dealId");
        var toStageId = context.GetGuid("toStageId") ?? throw Required("toStageId");
        var changedBy = context.GetGuid("changedBy") ?? Guid.Empty;
        return _dispatcher.SendAsync(new ChangeDealStageCommand(dealId, toStageId, changedBy), ct);
    }

    private static InvalidOperationException Required(string p)
        => new($"Action '{nameof(ChangeDealStageAction)}' requires parameter '{p}'.");
}

/// <summary>Действие Workflow: пометить сделку выигранной. Параметры: <c>dealId</c>, <c>changedBy</c>.</summary>
[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]
public sealed class WinDealAction : IWorkflowAction
{
    private readonly IDispatcher _dispatcher;

    public WinDealAction(IDispatcher dispatcher) => _dispatcher = dispatcher;

    public string Name => DealsActions.Win;

    public ValueTask ExecuteAsync(WorkflowActionContext context, CancellationToken ct)
    {
        var dealId = context.GetGuid("dealId")
            ?? throw new InvalidOperationException($"Action '{nameof(WinDealAction)}' requires parameter 'dealId'.");
        var changedBy = context.GetGuid("changedBy") ?? Guid.Empty;
        return _dispatcher.SendAsync(new WinDealCommand(dealId, changedBy), ct);
    }
}

/// <summary>Действие Workflow: сменить ответственного по сделке. Параметры: <c>dealId</c>, <c>newOwnerId</c>.</summary>
[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]
public sealed class AssignDealOwnerAction : IWorkflowAction
{
    private readonly IDispatcher _dispatcher;

    public AssignDealOwnerAction(IDispatcher dispatcher) => _dispatcher = dispatcher;

    public string Name => DealsActions.AssignOwner;

    public ValueTask ExecuteAsync(WorkflowActionContext context, CancellationToken ct)
    {
        var dealId = context.GetGuid("dealId")
            ?? throw new InvalidOperationException($"Action '{nameof(AssignDealOwnerAction)}' requires parameter 'dealId'.");
        var newOwnerId = context.GetGuid("newOwnerId")
            ?? throw new InvalidOperationException($"Action '{nameof(AssignDealOwnerAction)}' requires parameter 'newOwnerId'.");
        return _dispatcher.SendAsync(new AssignDealOwnerCommand(dealId, newOwnerId), ct);
    }
}
