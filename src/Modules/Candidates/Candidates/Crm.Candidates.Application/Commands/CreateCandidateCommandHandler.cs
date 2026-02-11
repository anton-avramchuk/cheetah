using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCandidateCommand, Guid>))]
public class CreateCandidateCommandHandler : ICommandHandler<CreateCandidateCommand, Guid>
{
    private readonly IRepository<Candidate, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateCandidateCommandHandler(IRepository<Candidate, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateCandidateCommand command, CancellationToken ct = default)
    {
        var entity = Candidate.Create(command.Name, command.Description);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}