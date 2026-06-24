using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Teams.Application.Exceptions;
using Cheetah.Modules.Teams.Domain.Entities;
using Cheetah.Modules.Teams.Domain.Specifications;

namespace Cheetah.Modules.Teams.Application.Roles;

// ── Создание ───────────────────────────────────────────────────────────────────────────────────

/// <summary>Создать роль участника.</summary>
public sealed record CreateTeamRoleCommand(string Name) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateTeamRoleCommand, Guid>))]
public sealed class CreateTeamRoleCommandHandler : ICommandHandler<CreateTeamRoleCommand, Guid>
{
    private readonly IRepository<TeamRole, Guid> _repository;

    public CreateTeamRoleCommandHandler(IRepository<TeamRole, Guid> repository)
        => _repository = repository;

    public async ValueTask<Guid> HandleAsync(CreateTeamRoleCommand command, CancellationToken ct = default)
    {
        if (await _repository.ExistsAsync(new TeamRoleByNameSpecification(command.Name), ct))
            throw new TeamsValidationException($"Role '{command.Name}' already exists");

        var role = TeamRole.Create(command.Name);
        _repository.Add(role);
        await _repository.SaveChangesAsync(ct);
        return role.Id;
    }
}

// ── Обновление ─────────────────────────────────────────────────────────────────────────────────

/// <summary>Переименовать роль.</summary>
public sealed record UpdateTeamRoleCommand(Guid Id, string Name) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateTeamRoleCommand>))]
public sealed class UpdateTeamRoleCommandHandler : ICommandHandler<UpdateTeamRoleCommand>
{
    private readonly IRepository<TeamRole, Guid> _repository;

    public UpdateTeamRoleCommandHandler(IRepository<TeamRole, Guid> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(UpdateTeamRoleCommand command, CancellationToken ct = default)
    {
        var role = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new TeamsValidationException($"Role '{command.Id}' not found");

        role.Rename(command.Name);
        await _repository.SaveChangesAsync(ct);
    }
}

// ── Удаление ───────────────────────────────────────────────────────────────────────────────────

/// <summary>Удалить роль.</summary>
public sealed record DeleteTeamRoleCommand(Guid Id) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteTeamRoleCommand>))]
public sealed class DeleteTeamRoleCommandHandler : ICommandHandler<DeleteTeamRoleCommand>
{
    private readonly IRepository<TeamRole, Guid> _repository;
    private readonly IRepository<TeamMembership, Guid> _membershipRepository;

    public DeleteTeamRoleCommandHandler(
        IRepository<TeamRole, Guid> repository, IRepository<TeamMembership, Guid> membershipRepository)
    {
        _repository = repository;
        _membershipRepository = membershipRepository;
    }

    public async ValueTask HandleAsync(DeleteTeamRoleCommand command, CancellationToken ct = default)
    {
        var role = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new TeamsValidationException($"Role '{command.Id}' not found");

        // Запрет удаления используемой роли — даёт понятную 400 вместо тихого каскадного удаления членств.
        if (await _membershipRepository.ExistsAsync(new TeamMembershipByRoleSpecification(command.Id), ct))
            throw new TeamsValidationException($"Role '{command.Id}' is assigned to team members and cannot be deleted");

        _repository.Delete(role);
        await _repository.SaveChangesAsync(ct);
    }
}
