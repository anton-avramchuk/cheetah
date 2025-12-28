using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Commands;

[Export(LifetimeType.Scoped)]
public class CreateTenantCommandHandler(
    IRepository<Tenant, Guid> repository,
    IEventBus eventBus
) : ICommandHandler<CreateTenantCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateTenantCommand command, CancellationToken cancellationToken = default)
    {
        var tenant = Tenant.Create(command.Name, command.Subdomain);

        await repository.InsertAsync(tenant, cancellationToken);

        foreach (var domainEvent in tenant.DomainEvents)
        {
            await eventBus.PublishAsync(domainEvent, cancellationToken);
        }

        return tenant.Id;
    }
}
