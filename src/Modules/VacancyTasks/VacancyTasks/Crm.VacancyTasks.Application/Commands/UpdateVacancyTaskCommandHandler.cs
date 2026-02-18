using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateVacancyTaskCommand>))]
public class UpdateVacancyTaskCommandHandler : ICommandHandler<UpdateVacancyTaskCommand>
{
    private readonly IRepository<VacancyTask, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateVacancyTaskCommandHandler(IRepository<VacancyTask, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateVacancyTaskCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<VacancyTask>(command.Id);

        entity.Update(command.Title, command.Description);
        entity.SetState(command.StateId);
        entity.SetPriority(command.PriorityId);
        entity.SetDueDate(command.DueDate);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}