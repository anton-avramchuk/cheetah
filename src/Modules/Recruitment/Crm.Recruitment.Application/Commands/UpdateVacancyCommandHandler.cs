using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateVacancyCommand>))]
public class UpdateVacancyCommandHandler : ICommandHandler<UpdateVacancyCommand>
{
    private readonly IRepository<Vacancy, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateVacancyCommandHandler(IRepository<Vacancy, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateVacancyCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Vacancy>(command.Id);

        entity.Update(command.Name, command.Description);
        entity.SetState(command.StateId);
        entity.SetCustomer(command.CustomerId);
        entity.SetPosition(command.PositionId);
        entity.SetStackItem(command.StackItemId);
        entity.SetWorkFormat(command.WorkFormatId);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}
