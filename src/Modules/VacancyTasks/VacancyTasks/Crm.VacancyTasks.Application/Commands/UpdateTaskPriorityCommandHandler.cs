using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateTaskPriorityCommand>))]
public class UpdateTaskPriorityCommandHandler : ICommandHandler<UpdateTaskPriorityCommand>
{
    private readonly IRepository<TaskPriority, Guid> _repository;

    public UpdateTaskPriorityCommandHandler(IRepository<TaskPriority, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UpdateTaskPriorityCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<TaskPriority>(command.Id);

        entity.Update(command.Name, command.Order, command.Color);
        await _repository.SaveChangesAsync(ct);
    }
}
