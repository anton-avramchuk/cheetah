using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Deals.Domain.Abstractions;
using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Application.Deals;

// ── Смена стадии ───────────────────────────────────────────────────────────────────────────

/// <summary>Переместить сделку на другую стадию её воронки.</summary>
public sealed record ChangeDealStageCommand(Guid DealId, Guid ToStageId, Guid ChangedBy) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<ChangeDealStageCommand>))]
public sealed class ChangeDealStageCommandHandler : ICommandHandler<ChangeDealStageCommand>
{
    private readonly IRepository<Deal, Guid> _deals;
    private readonly IPipelineRepository _pipelines;
    private readonly IEventBus _eventBus;

    public ChangeDealStageCommandHandler(
        IRepository<Deal, Guid> deals, IPipelineRepository pipelines, IEventBus eventBus)
    {
        _deals = deals;
        _pipelines = pipelines;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(ChangeDealStageCommand command, CancellationToken ct = default)
    {
        var deal = await _deals.GetByIdAsync(command.DealId, ct)
            ?? throw EntityNotFoundException.For<Deal>(command.DealId);
        var pipeline = await _pipelines.GetWithStagesAsync(deal.PipelineId, ct)
            ?? throw EntityNotFoundException.For<Pipeline>(deal.PipelineId);

        var target = pipeline.GetStage(command.ToStageId);
        deal.MoveToStage(target, command.ChangedBy); // доменные инварианты внутри

        await PublishAndSaveAsync(_deals, _eventBus, deal, ct);
    }

    internal static async ValueTask PublishAndSaveAsync(
        IRepository<Deal, Guid> deals, IEventBus eventBus, Deal deal, CancellationToken ct)
    {
        foreach (var e in deal.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        deal.ClearDomainEvents();
        await deals.SaveChangesAsync(ct);
    }
}

// ── Win ────────────────────────────────────────────────────────────────────────────────────

/// <summary>Перевести сделку в выигранную (терминальный статус).</summary>
public sealed record WinDealCommand(Guid DealId, Guid ChangedBy) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<WinDealCommand>))]
public sealed class WinDealCommandHandler : ICommandHandler<WinDealCommand>
{
    private readonly IRepository<Deal, Guid> _deals;
    private readonly IEventBus _eventBus;
    private readonly IStateMachineValidator<DealStatus> _stateMachine;

    public WinDealCommandHandler(
        IRepository<Deal, Guid> deals, IEventBus eventBus, IStateMachineValidator<DealStatus> stateMachine)
    {
        _deals = deals;
        _eventBus = eventBus;
        _stateMachine = stateMachine;
    }

    public async ValueTask HandleAsync(WinDealCommand command, CancellationToken ct = default)
    {
        var deal = await _deals.GetByIdAsync(command.DealId, ct)
            ?? throw EntityNotFoundException.For<Deal>(command.DealId);

        _stateMachine.ValidateTransition(deal.Status, DealStatus.Won);
        deal.Win(command.ChangedBy);

        await ChangeDealStageCommandHandler.PublishAndSaveAsync(_deals, _eventBus, deal, ct);
    }
}

// ── Lose ─────────────────────────────────────────────────────────────────────────────────────

/// <summary>Перевести сделку в проигранную (терминальный статус, требуется причина).</summary>
public sealed record LoseDealCommand(Guid DealId, string Reason, Guid ChangedBy) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<LoseDealCommand>))]
public sealed class LoseDealCommandHandler : ICommandHandler<LoseDealCommand>
{
    private readonly IRepository<Deal, Guid> _deals;
    private readonly IEventBus _eventBus;
    private readonly IStateMachineValidator<DealStatus> _stateMachine;

    public LoseDealCommandHandler(
        IRepository<Deal, Guid> deals, IEventBus eventBus, IStateMachineValidator<DealStatus> stateMachine)
    {
        _deals = deals;
        _eventBus = eventBus;
        _stateMachine = stateMachine;
    }

    public async ValueTask HandleAsync(LoseDealCommand command, CancellationToken ct = default)
    {
        var deal = await _deals.GetByIdAsync(command.DealId, ct)
            ?? throw EntityNotFoundException.For<Deal>(command.DealId);

        _stateMachine.ValidateTransition(deal.Status, DealStatus.Lost);
        deal.Lose(command.Reason, command.ChangedBy);

        await ChangeDealStageCommandHandler.PublishAndSaveAsync(_deals, _eventBus, deal, ct);
    }
}

// ── Сменить ответственного ──────────────────────────────────────────────────────────────────

/// <summary>Назначить нового ответственного по сделке.</summary>
public sealed record AssignDealOwnerCommand(Guid DealId, Guid NewOwnerId) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AssignDealOwnerCommand>))]
public sealed class AssignDealOwnerCommandHandler : ICommandHandler<AssignDealOwnerCommand>
{
    private readonly IRepository<Deal, Guid> _deals;
    private readonly IEventBus _eventBus;

    public AssignDealOwnerCommandHandler(IRepository<Deal, Guid> deals, IEventBus eventBus)
    {
        _deals = deals;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(AssignDealOwnerCommand command, CancellationToken ct = default)
    {
        var deal = await _deals.GetByIdAsync(command.DealId, ct)
            ?? throw EntityNotFoundException.For<Deal>(command.DealId);

        deal.AssignOwner(command.NewOwnerId);

        await ChangeDealStageCommandHandler.PublishAndSaveAsync(_deals, _eventBus, deal, ct);
    }
}
