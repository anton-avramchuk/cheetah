using System.Text.Json;
using Cheetah.Modules.FeatureManagement.Application.Abstractions;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Default.Contracts;
using Cheetah.Modules.FeatureManagement.Default.Entities;
using Cheetah.Modules.FeatureManagement.Domain.Entities;

namespace Cheetah.Modules.FeatureManagement.Default.Factories;

/// <summary>Фабрика конкретного флага «из коробки».</summary>
public sealed class FeatureFlagFactory : IFeatureFlagFactory<FeatureFlag, CreateFeatureFlagRequest>
{
    public FeatureFlag Create(CreateFeatureFlagRequest request)
    {
        var flag = FeatureFlag.Create(request.Key, request.Name, request.OwnerService, request.ValueType, request.Description);
        if (request.Variants.Count > 0)
            flag.SetVariants(request.Variants.Select(v => FeatureVariantDef.Create(flag.Id, v.Name, v.Value, v.Weight)));
        return flag;
    }

    public FeatureFlag CreateFromDescriptor(FeatureDefinitionDescriptor descriptor)
    {
        var flag = FeatureFlag.Create(descriptor.Key, descriptor.Name, descriptor.OwnerService, descriptor.ValueType, descriptor.Description);
        if (descriptor.ParentKey is not null)
            flag.SetParent(descriptor.ParentKey);
        if (descriptor.Variants is { Count: > 0 })
            flag.SetVariants(descriptor.Variants.Select(name => FeatureVariantDef.Create(flag.Id, name, null, 0)));
        return flag;
    }
}

/// <summary>Проектор конкретного флага «из коробки» в DTO.</summary>
public sealed class FeatureFlagProjector : IFeatureFlagProjector<FeatureFlag, FeatureFlagDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public FeatureFlagDto ToDto(FeatureFlag flag) => new()
    {
        Id = flag.Id,
        Key = flag.Key,
        Name = flag.Name,
        Description = flag.Description,
        OwnerService = flag.OwnerService,
        ParentKey = flag.ParentKey,
        Enabled = flag.Enabled,
        ValueType = flag.ValueType,
        IsActive = flag.IsActive,
        Rules = flag.Rules.OrderBy(r => r.Order).Select(r => new TargetingRuleDto(
            r.Order, r.FilterName, Deserialize(r.ParametersJson), r.ResultVariant, r.Negate)).ToArray(),
        Variants = flag.Variants.Select(v => new FeatureVariantDto(v.Name, v.Value, v.Weight)).ToArray(),
        CreatedAt = flag.CreatedAt,
        UpdatedAt = flag.UpdatedAt
    };

    private static IReadOnlyDictionary<string, object?> Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, object?>();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions) ?? new Dictionary<string, object?>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, object?>();
        }
    }
}
