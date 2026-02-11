using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateTaskPriorityCommand, Guid>))]
public class CreateTaskPriorityCommandHandler : ICommandHandler<CreateTaskPriorityCommand, Guid>
{
    private readonly IRepository<TaskPriority, Guid> _repository;

    public CreateTaskPriorityCommandHandler(IRepository<TaskPriority, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreateTaskPriorityCommand command, CancellationToken ct = default)
    {
        var entity = TaskPriority.Create(command.Name, command.Order, command.Color);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
