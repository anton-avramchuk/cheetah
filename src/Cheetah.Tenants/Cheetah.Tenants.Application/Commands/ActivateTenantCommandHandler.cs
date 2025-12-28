using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Commands;

[Export(LifetimeType.Scoped)]
public class ActivateTenantCommandHandler(
    IRepository<Tenant, Guid> repository,
    IEventBus eventBus
) : ICommandHandler<ActivateTenantCommand>
{
    public async ValueTask HandleAsync(ActivateTenantCommand command, CancellationToken cancellationToken = default)
    {
        var tenant = await repository.GetAsync(command.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant with ID {command.TenantId} not found");

        tenant.Activate();

        await repository.UpdateAsync(tenant, cancellationToken);

        foreach (var domainEvent in tenant.DomainEvents)
        {
            await eventBus.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
