using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateVacancyCommand, Guid>))]
public class CreateVacancyCommandHandler : ICommandHandler<CreateVacancyCommand, Guid>
{
    private readonly IRepository<Vacancy, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateVacancyCommandHandler(IRepository<Vacancy, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateVacancyCommand command, CancellationToken ct = default)
    {
        var entity = Vacancy.Create(command.Name, command.Description);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}
