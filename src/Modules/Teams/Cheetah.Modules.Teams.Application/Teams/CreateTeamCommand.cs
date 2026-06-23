using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Teams.Application.Abstractions;
using Cheetah.Modules.Teams.Application.Exceptions;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;
using Cheetah.Modules.Teams.Domain.Specifications;

namespace Cheetah.Modules.Teams.Application.Teams;

/// <summary>Создать команду из запроса наследника.</summary>
public sealed record CreateTeamCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateTeamRequestBase;

public class CreateTeamCommandHandler<TTeam, TCreateRequest>
    : ICommandHandler<CreateTeamCommand<TCreateRequest>, Guid>
    where TTeam : TeamBase
    where TCreateRequest : CreateTeamRequestBase
{
    private readonly ITeamFactory<TTeam, TCreateRequest> _factory;
    private readonly IRepository<TTeam, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateTeamCommandHandler(
        ITeamFactory<TTeam, TCreateRequest> factory,
        IRepository<TTeam, Guid> repository,
        IEventBus eventBus)
    {
        _factory = factory;
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateTeamCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        var name = command.Request.Name;
        if (await _repository.ExistsAsync(new TeamByNameSpecification<TTeam>(name), ct))
            throw new TeamsValidationException($"Team with name '{name}' already exists");

        var team = _factory.Create(command.Request);
        _repository.Add(team);
        await TeamHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, team, ct);

        return team.Id;
    }
}
