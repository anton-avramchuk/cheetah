using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteTaskPriorityCommand>))]
public class DeleteTaskPriorityCommandHandler : ICommandHandler<DeleteTaskPriorityCommand>
{
    private readonly IRepository<TaskPriority, Guid> _repository;

    public DeleteTaskPriorityCommandHandler(IRepository<TaskPriority, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteTaskPriorityCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<TaskPriority>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}
