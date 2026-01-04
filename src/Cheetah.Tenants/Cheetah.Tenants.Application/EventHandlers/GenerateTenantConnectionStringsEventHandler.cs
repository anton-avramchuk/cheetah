using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Core.Tenants.Services;
using Cheetah.Tenants.DataAccess.Repositories;
using Cheetah.Tenants.Events;
using Microsoft.Extensions.Logging;

namespace Cheetah.Tenants.Application.EventHandlers;

/// <summary>
/// Handles TenantCreatedEvent and automatically generates connection strings for all registered modules
/// This handler runs BEFORE TenantCreatedEventHandler to ensure connection strings exist before database creation
/// </summary>
[Export(LifetimeType.Scoped, typeof(IEventHandler<TenantCreatedEvent>))]
public class GenerateTenantConnectionStringsEventHandler : IEventHandler<TenantCreatedEvent>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantConnectionStringService _connectionStringService;
    private readonly ILogger<GenerateTenantConnectionStringsEventHandler> _logger;

    public GenerateTenantConnectionStringsEventHandler(
        ITenantRepository tenantRepository,
        ITenantConnectionStringService connectionStringService,
        ILogger<GenerateTenantConnectionStringsEventHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _connectionStringService = connectionStringService;
        _logger = logger;
    }

    public async ValueTask HandleAsync(TenantCreatedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Generating connection strings for tenant {TenantId} ({TenantName})",
            @event.TenantId, @event.Name);

        try
        {
            // Generate connection strings for all registered modules
            var connectionStrings = await _connectionStringService.GenerateAllConnectionStringsAsync(
                @event.TenantId,
                @event.Name);

            // Get tenant and add connection strings
            var tenant = await _tenantRepository.GetByIdAsync(@event.TenantId, ct);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found", @event.TenantId);
                return;
            }

            // Add connection string for each module
            foreach (var (moduleName, connectionString) in connectionStrings)
            {
                tenant.AddConnectionString(moduleName, connectionString, isDefault: false);
                _logger.LogInformation(
                    "Added connection string for module {ModuleName} to tenant {TenantId}",
                    moduleName, @event.TenantId);
            }

            await _tenantRepository.UpdateAsync(tenant, ct);

            _logger.LogInformation(
                "Successfully generated {Count} connection strings for tenant {TenantId}",
                connectionStrings.Count, @event.TenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to generate connection strings for tenant {TenantId}",
                @event.TenantId);
            throw;
        }
    }
}
