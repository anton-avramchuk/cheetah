using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.VacancyTasks.Domain;

namespace Crm.VacancyTasks.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteVacancyTaskCommand>))]
public class DeleteVacancyTaskCommandHandler : ICommandHandler<DeleteVacancyTaskCommand>
{
    private readonly IRepository<VacancyTask, Guid> _repository;

    public DeleteVacancyTaskCommandHandler(IRepository<VacancyTask, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteVacancyTaskCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<VacancyTask>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}