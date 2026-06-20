namespace Cheetah.FeatureManagement;

/// <summary>
/// Порт к источнику определений флагов. Абстракция НЕ знает, откуда они берутся.
/// Реализации (топология определяет, какая подключена):
/// <list type="bullet">
/// <item>в монолите / сервисе FeatureManagement — БД + кэш (<c>CachedFeatureDefinitionProvider</c>);</item>
/// <item>в микросервисе-потребителе — локальная in-memory реплика (<c>RemoteFeatureDefinitionProvider</c>):
/// pull при старте + push по событию с шины.</item>
/// </list>
/// Движок (<see cref="IFeatureManager"/>) и потребительский код от выбора реализации не зависят.
/// </summary>
public interface IFeatureDefinitionProvider
{
    /// <summary>Определение по ключу с учётом tenant-override; <c>null</c>, если флаг не зарегистрирован.</summary>
    ValueTask<FeatureDefinition?> GetAsync(string featureKey, Guid? tenantId, CancellationToken ct = default);

    /// <summary>Все определения (с учётом tenant-override) — для админки и реплики.</summary>
    ValueTask<IReadOnlyList<FeatureDefinition>> GetAllAsync(Guid? tenantId, CancellationToken ct = default);
}

/// <summary>Плоское read-определение флага для движка (НЕ доменная сущность).</summary>
public sealed record FeatureDefinition(
    string Key,
    bool Enabled,
    FeatureValueType ValueType,
    IReadOnlyList<TargetingRuleDefinition> Rules,
    IReadOnlyList<VariantDefinition> Variants)
{
    public static FeatureDefinition Disabled(string key)
        => new(key, false, FeatureValueType.Bool, [], []);
}

/// <summary>
/// Правило таргетинга для движка. <paramref name="Negate"/> моделирует deny-правило
/// («первое сработавшее»: deny → allow → percentage → default).
/// </summary>
public sealed record TargetingRuleDefinition(
    int Order,
    string FilterName,
    IReadOnlyDictionary<string, object?> Parameters,
    string? ResultVariant = null,
    bool Negate = false);

/// <summary>Вариант A/B с весом для детерминированного распределения.</summary>
public sealed record VariantDefinition(string Name, string? Value, int Weight);
