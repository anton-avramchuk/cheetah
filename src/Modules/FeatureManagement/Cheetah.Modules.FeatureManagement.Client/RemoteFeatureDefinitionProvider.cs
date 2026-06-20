using System.Collections.Concurrent;
using Cheetah.Core.Events;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.DomainEvents;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.FeatureManagement.Client;

/// <summary>
/// Реализация порта <see cref="IFeatureDefinitionProvider"/> для микросервиса-потребителя: локальная
/// in-memory реплика определений. Наполняется снимком при старте (pull) и обновляется по событиям с
/// шины (push). Горячий путь <see cref="IFeatureManager.IsEnabledAsync"/> — без сетевого хопа.
/// <para>MVP: реплика хранит глобальный снимок (tenantId=null); per-tenant override в реплике — follow-up.</para>
/// </summary>
public sealed class RemoteFeatureDefinitionProvider
    : IFeatureDefinitionProvider,
      IEventHandler<FeatureFlagChangedIntegrationEvent>,
      IEventHandler<FeatureFlagToggledIntegrationEvent>
{
    private readonly IFeatureCatalogClient _client;
    private readonly ILogger<RemoteFeatureDefinitionProvider> _logger;
    private volatile IReadOnlyDictionary<string, FeatureDefinition> _replica =
        new Dictionary<string, FeatureDefinition>(StringComparer.Ordinal);

    public RemoteFeatureDefinitionProvider(IFeatureCatalogClient client, ILogger<RemoteFeatureDefinitionProvider> logger)
    {
        _client = client;
        _logger = logger;
    }

    public ValueTask<FeatureDefinition?> GetAsync(string featureKey, Guid? tenantId, CancellationToken ct = default)
        => ValueTask.FromResult(_replica.GetValueOrDefault(featureKey));

    public ValueTask<IReadOnlyList<FeatureDefinition>> GetAllAsync(Guid? tenantId, CancellationToken ct = default)
        => ValueTask.FromResult<IReadOnlyList<FeatureDefinition>>(_replica.Values.ToArray());

    /// <summary>Полный pull снимка определений из каталога. Безопасно повторять.</summary>
    public async ValueTask RefreshAsync(CancellationToken ct = default)
    {
        var defs = await _client.PullDefinitionsAsync(null, ct);
        _replica = defs.ToDictionary(d => d.Key, StringComparer.Ordinal);
        _logger.LogDebug("Feature replica refreshed: {Count} definitions.", _replica.Count);
    }

    public ValueTask HandleAsync(FeatureFlagChangedIntegrationEvent @event, CancellationToken ct = default)
        => RefreshSafeAsync(ct);

    public ValueTask HandleAsync(FeatureFlagToggledIntegrationEvent @event, CancellationToken ct = default)
        => RefreshSafeAsync(ct);

    private async ValueTask RefreshSafeAsync(CancellationToken ct)
    {
        try
        {
            await RefreshAsync(ct);
        }
        catch (Exception ex)
        {
            // Реплика остаётся со stale-снимком до следующего события/перезапуска — фича не должна ронять сервис.
            _logger.LogWarning(ex, "Feature replica refresh on event failed; keeping stale snapshot.");
        }
    }
}
