using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;

namespace Cheetah.Admin.Modules.Clients.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateTariffCommand, Guid>))]
public class CreateTariffCommandHandler : ICommandHandler<CreateTariffCommand, Guid>
{
    private readonly ITariffRepository _repository;
    private readonly IEventBus _eventBus;

    public CreateTariffCommandHandler(ITariffRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateTariffCommand command, CancellationToken ct = default)
    {
        var entity = Tariff.Create(
            command.Name,
            command.Price,
            command.Currency,
            command.Description,
            command.IsActive);

        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);
        entity.ClearDomainEvents();

        return entity.Id;
    }
}
