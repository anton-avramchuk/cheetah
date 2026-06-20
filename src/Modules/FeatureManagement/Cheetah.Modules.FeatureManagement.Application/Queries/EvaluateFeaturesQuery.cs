using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Contracts;

namespace Cheetah.Modules.FeatureManagement.Application.Queries;

/// <summary>
/// Батч-оценка флагов для потребителей без локальной реплики (анти-N+1). Не зависит от конкретного
/// типа флага — работает через <see cref="IFeatureManager"/>, поэтому регистрируется через <c>[Export]</c>.
/// </summary>
public sealed record EvaluateFeaturesQuery(IReadOnlyList<string> Keys, FeatureContext Context)
    : IQuery<IReadOnlyDictionary<string, FeatureEvaluationDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<EvaluateFeaturesQuery, IReadOnlyDictionary<string, FeatureEvaluationDto>>))]
public sealed class EvaluateFeaturesQueryHandler
    : IQueryHandler<EvaluateFeaturesQuery, IReadOnlyDictionary<string, FeatureEvaluationDto>>
{
    private readonly IFeatureManager _features;

    public EvaluateFeaturesQueryHandler(IFeatureManager features) => _features = features;

    public async ValueTask<IReadOnlyDictionary<string, FeatureEvaluationDto>> HandleAsync(
        EvaluateFeaturesQuery query, CancellationToken ct = default)
    {
        var result = new Dictionary<string, FeatureEvaluationDto>(StringComparer.Ordinal);
        foreach (var key in query.Keys.Distinct(StringComparer.Ordinal))
        {
            var enabled = await _features.IsEnabledAsync(key, query.Context, ct);
            var variant = await _features.GetVariantAsync(key, query.Context, ct);
            result[key] = new FeatureEvaluationDto(enabled, variant?.Name);
        }

        return result;
    }
}
