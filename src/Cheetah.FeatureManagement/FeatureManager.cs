using Cheetah.Core.DependencyInjection;

namespace Cheetah.FeatureManagement;

/// <summary>
/// Движок оценки флагов. Чистая логика над определениями из <see cref="IFeatureDefinitionProvider"/>
/// (которые в горячем пути уже в кэше/реплике) — без I/O в самой оценке.
/// <para>
/// Стратегия «первое сработавшее» (§11.4 плана): kill-switch → правила по <c>Order</c>
/// (deny → allow → percentage → default) → значение по умолчанию.
/// </para>
/// </summary>
[Export(LifetimeType.Scoped, typeof(IFeatureManager))]
public sealed class FeatureManager : IFeatureManager
{
    private readonly IFeatureDefinitionProvider _provider;
    private readonly IReadOnlyDictionary<string, IFeatureFilter> _filters;

    public FeatureManager(IFeatureDefinitionProvider provider, IEnumerable<IFeatureFilter> filters)
    {
        _provider = provider;
        var map = new Dictionary<string, IFeatureFilter>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in filters)
            map[f.Name] = f; // последний с тем же именем выигрывает (пользовательский может заменить встроенный)
        _filters = map;
    }

    public async ValueTask<bool> IsEnabledAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default)
    {
        var (enabled, _) = await EvaluateAsync(featureKey, context ?? FeatureContext.Empty, ct);
        return enabled;
    }

    public async ValueTask<FeatureVariant?> GetVariantAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default)
    {
        var ctx = context ?? FeatureContext.Empty;
        var def = await _provider.GetAsync(featureKey, ctx.TenantId, ct);
        if (def is null || def.ValueType != FeatureValueType.Variant)
            return null;

        var (enabled, variantName) = await EvaluateCoreAsync(def, ctx, ct);
        if (!enabled)
            return null;

        var chosen = variantName ?? PickWeightedVariant(def, ctx);
        if (chosen is null)
            return null;

        var value = def.Variants.FirstOrDefault(v => v.Name == chosen)?.Value;
        return new FeatureVariant(chosen, value);
    }

    private async ValueTask<(bool Enabled, string? Variant)> EvaluateAsync(string key, FeatureContext ctx, CancellationToken ct)
    {
        var def = await _provider.GetAsync(key, ctx.TenantId, ct);
        return def is null ? (false, null) : await EvaluateCoreAsync(def, ctx, ct);
    }

    private async ValueTask<(bool Enabled, string? Variant)> EvaluateCoreAsync(FeatureDefinition def, FeatureContext ctx, CancellationToken ct)
    {
        // Kill-switch минует весь таргетинг.
        if (!def.Enabled)
            return (false, null);

        var rules = def.Rules;
        if (rules.Count == 0)
            return (true, null); // master-флаг без правил — раскатан на всех

        // «Первое сработавшее»: правила уже упорядочены по Order поставщиком.
        foreach (var rule in rules)
        {
            if (!_filters.TryGetValue(rule.FilterName, out var filter))
                continue; // неизвестный фильтр — правило пропускается (fail-safe)

            var matched = await filter.EvaluateAsync(
                new FeatureFilterContext(def.Key, ctx, rule.Parameters), ct);

            if (!matched)
                continue;

            return rule.Negate ? (false, null) : (true, rule.ResultVariant);
        }

        // Ни одно правило не сработало.
        return (false, null);
    }

    private static string? PickWeightedVariant(FeatureDefinition def, FeatureContext ctx)
    {
        if (def.Variants.Count == 0)
            return null;

        var totalWeight = def.Variants.Sum(v => Math.Max(0, v.Weight));
        if (totalWeight <= 0)
            return def.Variants[0].Name; // веса не заданы — детерминированно первый

        var subject = ctx.UserId?.ToString() ?? ctx.TenantId?.ToString() ?? string.Empty;
        var bucket = StableHash.Bucket($"{def.Key}:variant:{subject}") * totalWeight / 100;

        var cumulative = 0;
        foreach (var v in def.Variants)
        {
            cumulative += Math.Max(0, v.Weight);
            if (bucket < cumulative)
                return v.Name;
        }

        return def.Variants[^1].Name;
    }
}
