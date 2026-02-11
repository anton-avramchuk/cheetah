using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<MoveVacancyTaskCommand>))]
public class MoveVacancyTaskCommandHandler : ICommandHandler<MoveVacancyTaskCommand>
{
    private readonly IRepository<VacancyTask, Guid> _repository;

    public MoveVacancyTaskCommandHandler(IRepository<VacancyTask, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(MoveVacancyTaskCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<VacancyTask>(command.Id);

        entity.Move(command.StateId, command.Order);
        await _repository.SaveChangesAsync(ct);
    }
}
