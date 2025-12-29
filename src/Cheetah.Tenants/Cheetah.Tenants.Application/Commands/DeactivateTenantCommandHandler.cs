using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeactivateTenantCommand>))]
public class DeactivateTenantCommandHandler(
    IRepository<Tenant, Guid> repository,
    IEventBus eventBus
) : ICommandHandler<DeactivateTenantCommand>
{
    public async ValueTask HandleAsync(DeactivateTenantCommand command, CancellationToken cancellationToken = default)
    {
        var tenant = await repository.GetAsync(command.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant with ID {command.TenantId} not found");

        tenant.Deactivate();

        await repository.UpdateAsync(tenant, cancellationToken);

        foreach (var domainEvent in tenant.DomainEvents)
        {
            await eventBus.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
