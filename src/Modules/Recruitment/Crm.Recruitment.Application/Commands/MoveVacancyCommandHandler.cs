using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<MoveVacancyCommand>))]
public class MoveVacancyCommandHandler : ICommandHandler<MoveVacancyCommand>
{
    private readonly IRepository<Vacancy, Guid> _repository;

    public MoveVacancyCommandHandler(IRepository<Vacancy, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(MoveVacancyCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Vacancy>(command.Id);

        entity.Move(command.StateId, command.Order);
        await _repository.SaveChangesAsync(ct);
    }
}
