using System.Text.Json;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Domain.Entities;

namespace Cheetah.Modules.FeatureManagement.Infrastructure.Persistence;

/// <summary>
/// Проецирует доменный флаг в плоское <see cref="FeatureDefinition"/> для движка, разбирая
/// JSON-параметры правил и применяя tenant-override.
/// </summary>
internal static class FeatureDefinitionMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static FeatureDefinition ToDefinition(FeatureFlagBase flag, Guid? tenantId)
    {
        var variants = flag.Variants
            .Select(v => new VariantDefinition(v.Name, v.Value, v.Weight))
            .ToArray();

        // Override тенанта смотрится раньше глобального правила.
        if (tenantId is { } tid && flag.Overrides.FirstOrDefault(o => o.TenantId == tid) is { } ov)
        {
            var overrideRules = ParseOverrideRules(ov.RulesJson);
            return new FeatureDefinition(flag.Key, ov.Enabled, flag.ValueType, overrideRules, variants);
        }

        var rules = flag.Rules
            .OrderBy(r => r.Order)
            .Select(r => new TargetingRuleDefinition(
                r.Order, r.FilterName, ParseParameters(r.ParametersJson), r.ResultVariant, r.Negate))
            .ToArray();

        // Неактивный флаг трактуется как выключенный.
        var enabled = flag.Enabled && flag.IsActive;
        return new FeatureDefinition(flag.Key, enabled, flag.ValueType, rules, variants);
    }

    private static IReadOnlyDictionary<string, object?> ParseParameters(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return EmptyParams;
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions) ?? EmptyParams;
        }
        catch (JsonException)
        {
            return EmptyParams;
        }
    }

    private static IReadOnlyList<TargetingRuleDefinition> ParseOverrideRules(string? rulesJson)
    {
        if (string.IsNullOrWhiteSpace(rulesJson)) return [];
        try
        {
            var models = JsonSerializer.Deserialize<List<OverrideRuleModel>>(rulesJson, JsonOptions);
            if (models is null) return [];
            return models
                .OrderBy(m => m.Order)
                .Select(m => new TargetingRuleDefinition(
                    m.Order, m.FilterName ?? string.Empty, m.Parameters ?? EmptyParams, m.ResultVariant, m.Negate))
                .ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyParams =
        new Dictionary<string, object?>();

    private sealed record OverrideRuleModel(
        int Order, string? FilterName, Dictionary<string, object?>? Parameters, string? ResultVariant, bool Negate);
}
