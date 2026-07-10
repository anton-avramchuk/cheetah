using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Permissions.Catalog.Client;

/// <summary>
/// При старте микросервиса: сканирует свои сборки на [Permission]-атрибуты
/// и отправляет их в Permissions.Catalog.Api через <see cref="IPermissionsCatalogClient"/>.
/// </summary>
public sealed class RemoteRegistrySyncService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PermissionRegistry _registry;
    private readonly PermissionsCatalogClientOptions _options;
    private readonly ILogger<RemoteRegistrySyncService> _logger;

    public RemoteRegistrySyncService(
        IServiceProvider serviceProvider,
        PermissionRegistry registry,
        IOptions<PermissionsCatalogClientOptions> options,
        ILogger<RemoteRegistrySyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _registry = registry;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _registry.ScanAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        if (_registry.All.Count == 0)
        {
            _logger.LogDebug("No permissions to register; skip catalog sync");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IPermissionsCatalogClient>();

        // Группируем по module — каталог хранит источник permissions.
        var groups = _registry.All.GroupBy(d => d.Module);
        foreach (var group in groups)
        {
            try
            {
                var items = group.Select(d => new PermissionDefinitionDto(d.Key, d.Description, d.Module, d.Feature)).ToArray();
                await client.SyncAsync(new RegistrySyncRequest(group.Key, items), cancellationToken);
                _logger.LogInformation("Synced {Count} permissions for module {Module}", items.Length, group.Key);
            }
            catch (Exception ex) when (_options.ContinueOnFailure)
            {
                _logger.LogError(ex, "Failed to sync permissions for module {Module}", group.Key);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
