using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Teams.Application.Exceptions;
using Cheetah.Modules.Teams.Domain.Entities;
using Cheetah.Modules.Teams.Domain.Specifications;

namespace Cheetah.Modules.Teams.Application.Teams;

// ── Добавление участника ───────────────────────────────────────────────────────────────────────

/// <summary>Добавить участника в команду с ролью (идемпотентно — повтор меняет роль).</summary>
public sealed record AddTeamMemberCommand(Guid TeamId, Guid MemberId, Guid RoleId) : ICommand;

public class AddTeamMemberCommandHandler<TTeam> : ICommandHandler<AddTeamMemberCommand>
    where TTeam : TeamBase
{
    private readonly IRepository<TTeam, Guid> _repository;
    private readonly IEventBus _eventBus;

    public AddTeamMemberCommandHandler(IRepository<TTeam, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(AddTeamMemberCommand command, CancellationToken ct = default)
    {
        var team = await _repository.GetBySpecAsync(new TeamByIdSpecification<TTeam>(command.TeamId), ct)
            ?? throw new TeamsValidationException($"Team '{command.TeamId}' not found");

        team.AddMember(command.MemberId, command.RoleId);
        await TeamHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, team, ct);
    }
}

// ── Смена роли участника ───────────────────────────────────────────────────────────────────────

/// <summary>Изменить роль участника в команде.</summary>
public sealed record ChangeTeamMemberRoleCommand(Guid TeamId, Guid MemberId, Guid RoleId) : ICommand;

public class ChangeTeamMemberRoleCommandHandler<TTeam> : ICommandHandler<ChangeTeamMemberRoleCommand>
    where TTeam : TeamBase
{
    private readonly IRepository<TTeam, Guid> _repository;
    private readonly IEventBus _eventBus;

    public ChangeTeamMemberRoleCommandHandler(IRepository<TTeam, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(ChangeTeamMemberRoleCommand command, CancellationToken ct = default)
    {
        var team = await _repository.GetBySpecAsync(new TeamByIdSpecification<TTeam>(command.TeamId), ct)
            ?? throw new TeamsValidationException($"Team '{command.TeamId}' not found");

        try
        {
            team.ChangeMemberRole(command.MemberId, command.RoleId);
        }
        catch (InvalidOperationException ex)
        {
            throw new TeamsValidationException(ex.Message);
        }

        await TeamHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, team, ct);
    }
}

// ── Удаление участника ─────────────────────────────────────────────────────────────────────────

/// <summary>Удалить участника из команды.</summary>
public sealed record RemoveTeamMemberCommand(Guid TeamId, Guid MemberId) : ICommand;

public class RemoveTeamMemberCommandHandler<TTeam> : ICommandHandler<RemoveTeamMemberCommand>
    where TTeam : TeamBase
{
    private readonly IRepository<TTeam, Guid> _repository;
    private readonly IEventBus _eventBus;

    public RemoveTeamMemberCommandHandler(IRepository<TTeam, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(RemoveTeamMemberCommand command, CancellationToken ct = default)
    {
        var team = await _repository.GetBySpecAsync(new TeamByIdSpecification<TTeam>(command.TeamId), ct)
            ?? throw new TeamsValidationException($"Team '{command.TeamId}' not found");

        team.RemoveMember(command.MemberId);
        await TeamHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, team, ct);
    }
}
