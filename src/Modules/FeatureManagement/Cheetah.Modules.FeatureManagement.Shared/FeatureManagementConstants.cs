namespace Cheetah.Modules.FeatureManagement.Shared;

/// <summary>
/// Константы шаблонного модуля FeatureManagement: имя БД-подключения, ограничения длин, дефолтные
/// имена таблиц/схемы, префикс маршрутов и параметры кэша. Используются абстрактными базами как
/// значения по умолчанию (наследник может переопределить).
/// </summary>
public static class FeatureManagementConstants
{
    public const string ConnectionStringName = "FeatureManagement";

    public const int MaxKeyLength = 200;
    public const int MaxNameLength = 200;
    public const int MaxOwnerServiceLength = 100;
    public const int MaxFilterNameLength = 100;
    public const int MaxVariantNameLength = 100;

    public const string DefaultSchema = "features";
    public const string FlagsTableName = "FeatureFlags";
    public const string RulesTableName = "TargetingRules";
    public const string VariantsTableName = "FeatureVariants";
    public const string TenantOverridesTableName = "TenantOverrides";

    public const string DefaultRoutePrefix = "api/features";

    /// <summary>TTL кэша определений (фоллбэк к БД); инвалидация — по событию.</summary>
    public static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);
}
