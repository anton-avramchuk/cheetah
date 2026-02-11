using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCandidateApplicationCommand, Guid>))]
public class CreateCandidateApplicationCommandHandler : ICommandHandler<CreateCandidateApplicationCommand, Guid>
{
    private readonly IRepository<CandidateApplication, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateCandidateApplicationCommandHandler(IRepository<CandidateApplication, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateCandidateApplicationCommand command, CancellationToken ct = default)
    {
        var entity = CandidateApplication.Create(
            command.CandidateId, command.VacancyId, command.StageId, command.Order);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}
