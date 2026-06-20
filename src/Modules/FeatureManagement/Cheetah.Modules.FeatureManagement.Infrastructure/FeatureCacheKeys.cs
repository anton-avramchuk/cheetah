namespace Cheetah.Modules.FeatureManagement.Infrastructure;

/// <summary>Схема ключей кэша определений флагов.</summary>
internal static class FeatureCacheKeys
{
    public static string Definition(string featureKey, Guid? tenantId)
        => $"feature:{featureKey}:{(tenantId?.ToString() ?? "global")}";
}
