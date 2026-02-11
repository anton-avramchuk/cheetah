using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateVacancyTaskCommand, Guid>))]
public class CreateVacancyTaskCommandHandler : ICommandHandler<CreateVacancyTaskCommand, Guid>
{
    private readonly IRepository<VacancyTask, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateVacancyTaskCommandHandler(IRepository<VacancyTask, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateVacancyTaskCommand command, CancellationToken ct = default)
    {
        var entity = VacancyTask.Create(
            command.Title, command.VacancyId, command.StateId, command.Description,
            command.PriorityId, command.AssigneeId, command.DueDate);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}