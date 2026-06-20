using System.Text.Json;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Domain.Entities;

namespace Cheetah.Modules.FeatureManagement.Application;

/// <summary>Маппинг правил таргетинга DTO ↔ доменная сущность (параметры сериализуются в JSON для jsonb).</summary>
internal static class TargetingRuleMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IEnumerable<TargetingRule> ToEntities(Guid flagId, IEnumerable<TargetingRuleDto> dtos)
        => dtos.Select(d => TargetingRule.Create(
            flagId, d.Order, d.FilterName,
            JsonSerializer.Serialize(d.Parameters ?? new Dictionary<string, object?>(), JsonOptions),
            d.ResultVariant, d.Negate));

    public static string SerializeRules(IEnumerable<TargetingRuleDto> dtos)
        => JsonSerializer.Serialize(dtos, JsonOptions);
}
