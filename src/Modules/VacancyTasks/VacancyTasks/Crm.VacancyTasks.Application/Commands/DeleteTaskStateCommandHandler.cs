using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteTaskStateCommand>))]
public class DeleteTaskStateCommandHandler : ICommandHandler<DeleteTaskStateCommand>
{
    private readonly IRepository<TaskState, Guid> _repository;
    private readonly IReadOnlyRepository<VacancyTask, Guid> _vacancyTaskRepository;

    public DeleteTaskStateCommandHandler(
        IRepository<TaskState, Guid> repository,
        IRepository<VacancyTask, Guid> vacancyTaskRepository)
    {
        _repository = repository;
        _vacancyTaskRepository = vacancyTaskRepository;
    }

    public async ValueTask HandleAsync(DeleteTaskStateCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<TaskState>(command.Id);

        var isInUse = await _vacancyTaskRepository.AsNoTrackingQueryable()
            .AnyAsync(t => t.StateId == command.Id, ct);

        if (isInUse)
            throw new InvalidOperationException($"Cannot delete TaskState '{entity.Name}' because it is in use by one or more vacancy tasks.");

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}
