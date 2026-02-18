using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteVacancyStateCommand>))]
public class DeleteVacancyStateCommandHandler : ICommandHandler<DeleteVacancyStateCommand>
{
    private readonly IRepository<VacancyState, Guid> _repository;
    private readonly IReadOnlyRepository<Vacancy, Guid> _vacancyRepository;

    public DeleteVacancyStateCommandHandler(
        IRepository<VacancyState, Guid> repository,
        IReadOnlyRepository<Vacancy, Guid> vacancyRepository)
    {
        _repository = repository;
        _vacancyRepository = vacancyRepository;
    }

    public async ValueTask HandleAsync(DeleteVacancyStateCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<VacancyState>(command.Id);

        var isInUse = await _vacancyRepository.AsNoTrackingQueryable()
            .AnyAsync(v => v.StateId == command.Id, ct);

        if (isInUse)
            throw new InvalidOperationException($"Cannot delete VacancyState '{entity.Name}' because it is in use by one or more vacancies.");

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}
