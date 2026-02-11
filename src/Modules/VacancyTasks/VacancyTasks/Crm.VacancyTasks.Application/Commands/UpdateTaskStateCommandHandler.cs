using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateTaskStateCommand>))]
public class UpdateTaskStateCommandHandler : ICommandHandler<UpdateTaskStateCommand>
{
    private readonly IRepository<TaskState, Guid> _repository;

    public UpdateTaskStateCommandHandler(IRepository<TaskState, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UpdateTaskStateCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<TaskState>(command.Id);

        entity.Update(command.Name, command.Order, command.Color, command.IsDefault);
        await _repository.SaveChangesAsync(ct);
    }
}
