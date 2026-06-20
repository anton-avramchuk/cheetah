using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.FeatureManagement.Client;

/// <summary>
/// При старте: (1) регистрирует флаги сервиса в каталоге (registry/sync), (2) если включена локальная
/// реплика — делает первый pull снимка. Обе операции <c>ContinueOnFailure</c> — недоступность каталога
/// НЕ валит хост (флаги работают со значениями по умолчанию / stale-репликой).
/// </summary>
public sealed class FeatureClientHostedService : IHostedService
{
    private readonly IEnumerable<FeatureRegistrationContribution> _contributions;
    private readonly IFeatureCatalogClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FeatureClientHostedService> _logger;

    public FeatureClientHostedService(
        IEnumerable<FeatureRegistrationContribution> contributions,
        IFeatureCatalogClient client,
        IServiceProvider serviceProvider,
        ILogger<FeatureClientHostedService> logger)
    {
        _contributions = contributions;
        _client = client;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var descriptors = _contributions.SelectMany(c => c.Descriptors).ToArray();
        if (descriptors.Length > 0)
        {
            try
            {
                await _client.SyncAsync(descriptors, cancellationToken);
                _logger.LogInformation("Registered {Count} feature flags in catalog.", descriptors.Length);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Feature registry sync failed; flags fall back to defaults.");
            }
        }

        if (_serviceProvider.GetService<RemoteFeatureDefinitionProvider>() is { } replica)
        {
            try
            {
                await replica.RefreshAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Initial feature replica pull failed; starting with empty replica.");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
