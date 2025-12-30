using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Identity.Application.Services;
using Cheetah.Tenants.Events;
using Microsoft.Extensions.Logging;

namespace Cheetah.Identity.Application.EventHandlers;

/// <summary>
/// Handles TenantCreatedEvent to automatically create and migrate Identity database for new tenant
/// </summary>
[Export(LifetimeType.Scoped, typeof(TenantCreatedEventHandler))]
public class TenantCreatedEventHandler : IEventHandler<TenantCreatedEvent>
{
    private readonly IDatabaseMigrationManager _migrationManager;
    private readonly ILogger<TenantCreatedEventHandler> _logger;

    public TenantCreatedEventHandler(
        IDatabaseMigrationManager migrationManager,
        ILogger<TenantCreatedEventHandler> logger)
    {
        _migrationManager = migrationManager;
        _logger = logger;
    }

    public async ValueTask HandleAsync(TenantCreatedEvent @event, CancellationToken ct)
    {
        _logger.LogInformation(
            "TenantCreatedEvent received for tenant {TenantId} ({TenantName}). Creating Identity database...",
            @event.TenantId, @event.Name);

        try
        {
            await _migrationManager.CreateTenantDatabaseAsync(@event.TenantId, ct);

            _logger.LogInformation(
                "Successfully created Identity database for tenant {TenantId} ({TenantName})",
                @event.TenantId, @event.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create Identity database for tenant {TenantId} ({TenantName})",
                @event.TenantId, @event.Name);
            throw;
        }
    }
}
