using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Tenants.Application.DatabaseMigrations;
using Cheetah.Tenants.Events;
using Microsoft.Extensions.Logging;

namespace Cheetah.Tenants.Application.EventHandlers;

/// <summary>
/// Handles TenantCreatedEvent and creates the tenant database
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

    public async ValueTask HandleAsync(TenantCreatedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating database for tenant {TenantId} ({TenantName})", @event.TenantId, @event.Name);

        try
        {
            await _migrationManager.CreateTenantDatabaseAsync(@event.TenantId, ct);
            _logger.LogInformation("Successfully created database for tenant {TenantId}", @event.TenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database for tenant {TenantId}", @event.TenantId);
            throw;
        }
    }
}
