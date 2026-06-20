namespace Cheetah.FeatureManagement.Tests;

/// <summary>In-memory поставщик определений для тестов движка.</summary>
internal sealed class FakeDefinitionProvider : IFeatureDefinitionProvider
{
    private readonly Dictionary<string, FeatureDefinition> _defs;

    public FakeDefinitionProvider(params FeatureDefinition[] defs)
        => _defs = defs.ToDictionary(d => d.Key, StringComparer.Ordinal);

    public ValueTask<FeatureDefinition?> GetAsync(string featureKey, Guid? tenantId, CancellationToken ct = default)
        => ValueTask.FromResult(_defs.GetValueOrDefault(featureKey));

    public ValueTask<IReadOnlyList<FeatureDefinition>> GetAllAsync(Guid? tenantId, CancellationToken ct = default)
        => ValueTask.FromResult<IReadOnlyList<FeatureDefinition>>(_defs.Values.ToArray());
}

/// <summary>Фильтр с фиксированным именем и заранее заданным ответом.</summary>
internal sealed class StubFilter(string name, bool result) : IFeatureFilter
{
    public string Name => name;
    public ValueTask<bool> EvaluateAsync(FeatureFilterContext context, CancellationToken ct)
        => ValueTask.FromResult(result);
}
