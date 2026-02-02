using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Repositories;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateVacancyCommand>))]
public class UpdateVacancyCommandHandler : ICommandHandler<UpdateVacancyCommand>
{
    private readonly IVacancyRepository _repository;
    private readonly IEventBus _eventBus;

    public UpdateVacancyCommandHandler(IVacancyRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateVacancyCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Vacancy>(command.Id);

        entity.Update(command.Name, command.Description);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}