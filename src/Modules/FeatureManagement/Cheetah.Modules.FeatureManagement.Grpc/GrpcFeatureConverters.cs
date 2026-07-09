using System.Text.Json;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Grpc.Protos;

namespace Cheetah.Modules.FeatureManagement.Grpc;

/// <summary>
/// Ручные конверсии proto ↔ типы движка. gRPC-срез намеренно не тянет IObjectMapper:
/// словари атрибутов/параметров (object? → JSON-строки) мапятся нетривиально.
/// </summary>
public static class GrpcFeatureConverters
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>proto-контекст → <see cref="FeatureContext"/>. Кривой Guid → FormatException → InvalidArgument.</summary>
    public static FeatureContext ToContext(FeatureContextProto? proto)
    {
        if (proto is null) return FeatureContext.Empty;

        return new FeatureContext
        {
            UserId = proto.HasUserId && proto.UserId.Length > 0 ? Guid.Parse(proto.UserId) : null,
            TenantId = proto.HasTenantId && proto.TenantId.Length > 0 ? Guid.Parse(proto.TenantId) : null,
            Roles = proto.Roles.ToArray(),
            Attributes = proto.AttributesJson.ToDictionary(
                kv => kv.Key, kv => DeserializeJsonValue(kv.Value), StringComparer.Ordinal)
        };
    }

    /// <summary>Определение движка → proto (для снимка реплики).</summary>
    public static FeatureDefinitionProto ToProto(FeatureDefinition definition)
    {
        var proto = new FeatureDefinitionProto
        {
            Key = definition.Key,
            Enabled = definition.Enabled,
            ValueType = (int)definition.ValueType
        };

        foreach (var rule in definition.Rules)
        {
            var ruleProto = new TargetingRuleProto
            {
                Order = rule.Order,
                FilterName = rule.FilterName,
                Negate = rule.Negate
            };
            if (rule.ResultVariant is not null)
                ruleProto.ResultVariant = rule.ResultVariant;
            foreach (var (key, value) in rule.Parameters)
                ruleProto.ParametersJson[key] = JsonSerializer.Serialize(value, JsonOptions);

            proto.Rules.Add(ruleProto);
        }

        foreach (var variant in definition.Variants)
        {
            var variantProto = new VariantDefProto { Name = variant.Name, Weight = variant.Weight };
            if (variant.Value is not null)
                variantProto.Value = variant.Value;

            proto.Variants.Add(variantProto);
        }

        return proto;
    }

    /// <summary>proto → определение движка (для будущего gRPC-pull реплики; симметрична <see cref="ToProto"/>).</summary>
    public static FeatureDefinition ToDefinition(FeatureDefinitionProto proto)
        => new(
            proto.Key,
            proto.Enabled,
            (FeatureValueType)proto.ValueType,
            proto.Rules
                .Select(r => new TargetingRuleDefinition(
                    r.Order,
                    r.FilterName,
                    r.ParametersJson.ToDictionary(
                        kv => kv.Key, kv => DeserializeJsonValue(kv.Value), StringComparer.Ordinal),
                    r.HasResultVariant ? r.ResultVariant : null,
                    r.Negate))
                .ToArray(),
            proto.Variants
                .Select(v => new VariantDefinition(v.Name, v.HasValue ? v.Value : null, v.Weight))
                .ToArray());

    private static object? DeserializeJsonValue(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<object?>(json, JsonOptions);
        }
        catch (JsonException)
        {
            // Невалидный JSON трактуем как сырую строку — тот же fail-soft, что у jsonb-параметров.
            return json;
        }
    }
}
