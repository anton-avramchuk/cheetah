using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateVacancyStateCommand, Guid>))]
public class CreateVacancyStateCommandHandler : ICommandHandler<CreateVacancyStateCommand, Guid>
{
    private readonly IRepository<VacancyState, Guid> _repository;

    public CreateVacancyStateCommandHandler(IRepository<VacancyState, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreateVacancyStateCommand command, CancellationToken ct = default)
    {
        var entity = VacancyState.Create(command.Name, command.Order, command.Color, command.IsDefault);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}
