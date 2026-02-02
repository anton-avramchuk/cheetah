using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Repositories;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteVacancyCommand>))]
public class DeleteVacancyCommandHandler : ICommandHandler<DeleteVacancyCommand>
{
    private readonly IVacancyRepository _repository;

    public DeleteVacancyCommandHandler(IVacancyRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteVacancyCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Vacancy>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}