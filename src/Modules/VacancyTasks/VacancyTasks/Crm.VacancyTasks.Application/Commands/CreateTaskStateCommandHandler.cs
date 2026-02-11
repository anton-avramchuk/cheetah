using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateTaskStateCommand, Guid>))]
public class CreateTaskStateCommandHandler : ICommandHandler<CreateTaskStateCommand, Guid>
{
    private readonly IRepository<TaskState, Guid> _repository;

    public CreateTaskStateCommandHandler(IRepository<TaskState, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreateTaskStateCommand command, CancellationToken ct = default)
    {
        var entity = TaskState.Create(command.Name, command.Order, command.Color, command.IsDefault);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
