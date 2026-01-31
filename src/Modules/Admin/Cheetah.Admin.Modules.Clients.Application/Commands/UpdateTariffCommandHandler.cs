using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;

namespace Cheetah.Admin.Modules.Clients.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateTariffCommand>))]
public class UpdateTariffCommandHandler : ICommandHandler<UpdateTariffCommand>
{
    private readonly ITariffRepository _repository;
    private readonly IEventBus _eventBus;

    public UpdateTariffCommandHandler(ITariffRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateTariffCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw EntityNotFoundException.For<Tariff>(command.Id);

        entity.Update(
            command.Name,
            command.Price,
            command.Currency,
            command.Description,
            command.IsActive);

        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();
    }
}
