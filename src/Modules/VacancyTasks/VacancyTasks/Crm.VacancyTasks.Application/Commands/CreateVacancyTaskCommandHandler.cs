using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;

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
        const int maxRetries = 3;

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            var maxNumber = await _repository.AsNoTrackingQueryable()
                .Where(t => t.VacancyId == command.VacancyId)
                .MaxAsync(t => (int?)t.Number, ct) ?? 0;

            var entity = VacancyTask.Create(
                command.Title, command.VacancyId, command.StateId, maxNumber + 1,
                command.Description, command.PriorityId, command.AssigneeId, command.DueDate);
            _repository.Add(entity);

            try
            {
                await _repository.SaveChangesAsync(ct);

                foreach (var domainEvent in entity.DomainEvents)
                    await _eventBus.PublishAsync(domainEvent, ct);
                entity.ClearDomainEvents();

                return entity.Id;
            }
            catch (DbUpdateException) when (attempt < maxRetries - 1)
            {
                _repository.Delete(entity);
            }
        }

        throw new InvalidOperationException("Failed to generate unique task number after multiple attempts.");
    }
}