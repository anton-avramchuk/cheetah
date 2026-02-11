using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateCandidateCommand>))]
public class UpdateCandidateCommandHandler : ICommandHandler<UpdateCandidateCommand>
{
    private readonly IRepository<Candidate, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateCandidateCommandHandler(IRepository<Candidate, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateCandidateCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Candidate>(command.Id);

        entity.Update(command.Name, command.Description);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}