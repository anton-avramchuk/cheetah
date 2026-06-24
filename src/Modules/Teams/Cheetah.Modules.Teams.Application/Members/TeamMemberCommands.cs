using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Teams.Application.Exceptions;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.Members;

// ── Создание ───────────────────────────────────────────────────────────────────────────────────

/// <summary>Создать участника (справочник людей).</summary>
public sealed record CreateTeamMemberCommand(string Name) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateTeamMemberCommand, Guid>))]
public sealed class CreateTeamMemberCommandHandler : ICommandHandler<CreateTeamMemberCommand, Guid>
{
    private readonly IRepository<TeamMember, Guid> _repository;

    public CreateTeamMemberCommandHandler(IRepository<TeamMember, Guid> repository)
        => _repository = repository;

    public async ValueTask<Guid> HandleAsync(CreateTeamMemberCommand command, CancellationToken ct = default)
    {
        var member = TeamMember.Create(command.Name);
        _repository.Add(member);
        await _repository.SaveChangesAsync(ct);
        return member.Id;
    }
}

// ── Обновление ─────────────────────────────────────────────────────────────────────────────────

/// <summary>Обновить участника (имя).</summary>
public sealed record UpdateTeamMemberCommand(Guid Id, string Name) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateTeamMemberCommand>))]
public sealed class UpdateTeamMemberCommandHandler : ICommandHandler<UpdateTeamMemberCommand>
{
    private readonly IRepository<TeamMember, Guid> _repository;

    public UpdateTeamMemberCommandHandler(IRepository<TeamMember, Guid> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(UpdateTeamMemberCommand command, CancellationToken ct = default)
    {
        var member = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new TeamsValidationException($"Member '{command.Id}' not found");

        member.Rename(command.Name);
        await _repository.SaveChangesAsync(ct);
    }
}

// ── Удаление ───────────────────────────────────────────────────────────────────────────────────

/// <summary>Удалить участника из справочника.</summary>
public sealed record DeleteTeamMemberCommand(Guid Id) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteTeamMemberCommand>))]
public sealed class DeleteTeamMemberCommandHandler : ICommandHandler<DeleteTeamMemberCommand>
{
    private readonly IRepository<TeamMember, Guid> _repository;

    public DeleteTeamMemberCommandHandler(IRepository<TeamMember, Guid> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(DeleteTeamMemberCommand command, CancellationToken ct = default)
    {
        var member = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new TeamsValidationException($"Member '{command.Id}' not found");

        _repository.Delete(member);
        await _repository.SaveChangesAsync(ct);
    }
}
