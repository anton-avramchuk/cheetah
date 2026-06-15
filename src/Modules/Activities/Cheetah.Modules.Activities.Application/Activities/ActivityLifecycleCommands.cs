using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Activities.Application.Exceptions;
using Cheetah.Modules.Activities.Domain.Entities;
using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Application.Activities;

// ── Перевод в работу ────────────────────────────────────────────────────────────────────────

/// <summary>Перевести активность в статус InProgress.</summary>
public sealed record StartActivityCommand(Guid Id) : ICommand;

public class StartActivityCommandHandler<TActivity> : ICommandHandler<StartActivityCommand>
    where TActivity : ActivityBase
{
    private readonly IRepository<TActivity, Guid> _repository;
    private readonly IStateMachineValidator<ActivityStatus> _stateMachine;

    public StartActivityCommandHandler(
        IRepository<TActivity, Guid> repository, IStateMachineValidator<ActivityStatus> stateMachine)
    {
        _repository = repository;
        _stateMachine = stateMachine;
    }

    public async ValueTask HandleAsync(StartActivityCommand command, CancellationToken ct = default)
    {
        var activity = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new ActivityValidationException($"Activity '{command.Id}' not found");

        _stateMachine.ValidateTransition(activity.Status, ActivityStatus.InProgress);
        activity.Start();
        await _repository.SaveChangesAsync(ct);
    }
}

// ── Завершение ──────────────────────────────────────────────────────────────────────────────

/// <summary>Завершить активность (терминальный статус Done).</summary>
public sealed record CompleteActivityCommand(Guid Id, Guid CompletedBy, string? Result) : ICommand;

public class CompleteActivityCommandHandler<TActivity> : ICommandHandler<CompleteActivityCommand>
    where TActivity : ActivityBase
{
    private readonly IRepository<TActivity, Guid> _repository;
    private readonly IEventBus _eventBus;
    private readonly IStateMachineValidator<ActivityStatus> _stateMachine;

    public CompleteActivityCommandHandler(
        IRepository<TActivity, Guid> repository, IEventBus eventBus,
        IStateMachineValidator<ActivityStatus> stateMachine)
    {
        _repository = repository;
        _eventBus = eventBus;
        _stateMachine = stateMachine;
    }

    public async ValueTask HandleAsync(CompleteActivityCommand command, CancellationToken ct = default)
    {
        var activity = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new ActivityValidationException($"Activity '{command.Id}' not found");

        _stateMachine.ValidateTransition(activity.Status, ActivityStatus.Done);
        activity.Complete(command.CompletedBy, command.Result);
        await SaveAndPublishAsync(_repository, _eventBus, activity, ct);
    }

    internal static async ValueTask SaveAndPublishAsync(
        IRepository<TActivity, Guid> repository, IEventBus eventBus, TActivity activity, CancellationToken ct)
    {
        await repository.SaveChangesAsync(ct);
        foreach (var e in activity.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        activity.ClearDomainEvents();
    }
}

// ── Отмена ──────────────────────────────────────────────────────────────────────────────────

/// <summary>Отменить активность (терминальный статус Canceled).</summary>
public sealed record CancelActivityCommand(Guid Id) : ICommand;

public class CancelActivityCommandHandler<TActivity> : ICommandHandler<CancelActivityCommand>
    where TActivity : ActivityBase
{
    private readonly IRepository<TActivity, Guid> _repository;
    private readonly IEventBus _eventBus;
    private readonly IStateMachineValidator<ActivityStatus> _stateMachine;

    public CancelActivityCommandHandler(
        IRepository<TActivity, Guid> repository, IEventBus eventBus,
        IStateMachineValidator<ActivityStatus> stateMachine)
    {
        _repository = repository;
        _eventBus = eventBus;
        _stateMachine = stateMachine;
    }

    public async ValueTask HandleAsync(CancelActivityCommand command, CancellationToken ct = default)
    {
        var activity = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new ActivityValidationException($"Activity '{command.Id}' not found");

        _stateMachine.ValidateTransition(activity.Status, ActivityStatus.Canceled);
        activity.Cancel();
        await CompleteActivityCommandHandler<TActivity>.SaveAndPublishAsync(_repository, _eventBus, activity, ct);
    }
}

// ── Смена исполнителя ──────────────────────────────────────────────────────────────────────

/// <summary>Назначить нового исполнителя активности.</summary>
public sealed record ReassignActivityCommand(Guid Id, Guid NewAssigneeId) : ICommand;

public class ReassignActivityCommandHandler<TActivity> : ICommandHandler<ReassignActivityCommand>
    where TActivity : ActivityBase
{
    private readonly IRepository<TActivity, Guid> _repository;
    private readonly IEventBus _eventBus;

    public ReassignActivityCommandHandler(IRepository<TActivity, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(ReassignActivityCommand command, CancellationToken ct = default)
    {
        var activity = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new ActivityValidationException($"Activity '{command.Id}' not found");

        activity.Reassign(command.NewAssigneeId);
        await CompleteActivityCommandHandler<TActivity>.SaveAndPublishAsync(_repository, _eventBus, activity, ct);
    }
}
