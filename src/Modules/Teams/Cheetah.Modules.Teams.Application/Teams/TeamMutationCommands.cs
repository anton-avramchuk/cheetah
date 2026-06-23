using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Teams.Application.Exceptions;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.Teams;

// ── Переименование ─────────────────────────────────────────────────────────────────────────────

/// <summary>Переименовать команду.</summary>
public sealed record UpdateTeamCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand
    where TUpdateRequest : UpdateTeamRequestBase;

public class UpdateTeamCommandHandler<TTeam, TUpdateRequest>
    : ICommandHandler<UpdateTeamCommand<TUpdateRequest>>
    where TTeam : TeamBase
    where TUpdateRequest : UpdateTeamRequestBase
{
    private readonly IRepository<TTeam, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateTeamCommandHandler(IRepository<TTeam, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateTeamCommand<TUpdateRequest> command, CancellationToken ct = default)
    {
        var team = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new TeamsValidationException($"Team '{command.Id}' not found");

        team.Rename(command.Request.Name);
        await TeamHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, team, ct);
    }
}

// ── Деактивация ────────────────────────────────────────────────────────────────────────────────

/// <summary>Деактивировать команду (расформировать/в архив).</summary>
public sealed record DeactivateTeamCommand(Guid Id) : ICommand;

public class DeactivateTeamCommandHandler<TTeam> : ICommandHandler<DeactivateTeamCommand>
    where TTeam : TeamBase
{
    private readonly IRepository<TTeam, Guid> _repository;
    private readonly IEventBus _eventBus;

    public DeactivateTeamCommandHandler(IRepository<TTeam, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(DeactivateTeamCommand command, CancellationToken ct = default)
    {
        var team = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new TeamsValidationException($"Team '{command.Id}' not found");

        team.Deactivate();
        await TeamHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, team, ct);
    }
}

// ── Активация ──────────────────────────────────────────────────────────────────────────────────

/// <summary>Активировать ранее деактивированную команду.</summary>
public sealed record ActivateTeamCommand(Guid Id) : ICommand;

public class ActivateTeamCommandHandler<TTeam> : ICommandHandler<ActivateTeamCommand>
    where TTeam : TeamBase
{
    private readonly IRepository<TTeam, Guid> _repository;
    private readonly IEventBus _eventBus;

    public ActivateTeamCommandHandler(IRepository<TTeam, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(ActivateTeamCommand command, CancellationToken ct = default)
    {
        var team = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new TeamsValidationException($"Team '{command.Id}' not found");

        team.Activate();
        await TeamHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, team, ct);
    }
}

// ── Удаление ───────────────────────────────────────────────────────────────────────────────────

/// <summary>Удалить команду физически (жёсткое удаление; мягкое — через Deactivate).</summary>
public sealed record DeleteTeamCommand(Guid Id) : ICommand;

public class DeleteTeamCommandHandler<TTeam> : ICommandHandler<DeleteTeamCommand>
    where TTeam : TeamBase
{
    private readonly IRepository<TTeam, Guid> _repository;

    public DeleteTeamCommandHandler(IRepository<TTeam, Guid> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(DeleteTeamCommand command, CancellationToken ct = default)
    {
        var team = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new TeamsValidationException($"Team '{command.Id}' not found");

        _repository.Delete(team);
        await _repository.SaveChangesAsync(ct);
    }
}
