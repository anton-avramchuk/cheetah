using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateVacancyStateCommand>))]
public class UpdateVacancyStateCommandHandler : ICommandHandler<UpdateVacancyStateCommand>
{
    private readonly IRepository<VacancyState, Guid> _repository;

    public UpdateVacancyStateCommandHandler(IRepository<VacancyState, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UpdateVacancyStateCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<VacancyState>(command.Id);

        entity.Update(command.Name, command.Order, command.IsDefault, command.Color);
        await _repository.SaveChangesAsync(ct);
    }
}
