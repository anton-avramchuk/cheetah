using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteVacancyCommand>))]
public class DeleteVacancyCommandHandler : ICommandHandler<DeleteVacancyCommand>
{
    private readonly IRepository<Vacancy, Guid> _repository;

    public DeleteVacancyCommandHandler(IRepository<Vacancy, Guid> repository)
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
