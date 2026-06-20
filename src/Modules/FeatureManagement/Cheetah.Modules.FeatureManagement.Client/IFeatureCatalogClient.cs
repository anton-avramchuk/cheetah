using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Contracts;

namespace Cheetah.Modules.FeatureManagement.Client;

/// <summary>
/// HTTP-клиент к сервису FeatureManagement (server-to-server). Используется для регистрации флагов при
/// старте, удалённой оценки и снимка определений для локальной реплики.
/// </summary>
public interface IFeatureCatalogClient
{
    /// <summary>Идемпотентный upsert дескрипторов флагов в каталог (registry/sync).</summary>
    ValueTask SyncAsync(IReadOnlyList<FeatureDefinitionDescriptor> descriptors, CancellationToken ct = default);

    /// <summary>Удалённая батч-оценка (для потребителей без локальной реплики).</summary>
    ValueTask<IReadOnlyDictionary<string, FeatureEvaluationDto>> EvaluateAsync(
        IReadOnlyList<string> keys, FeatureContext context, CancellationToken ct = default);

    /// <summary>Снимок всех определений (для наполнения <c>RemoteFeatureDefinitionProvider</c>).</summary>
    ValueTask<IReadOnlyList<FeatureDefinition>> PullDefinitionsAsync(Guid? tenantId = null, CancellationToken ct = default);
}
