namespace Cheetah.FeatureManagement;

/// <summary>
/// Стратегия таргетинга — точка расширения движка. Встроенные: Percentage, Users, Tenants, Roles,
/// TimeWindow, JsonLogic. Приложение регистрирует свои фильтры через
/// <c>[Export(LifetimeType.Singleton, typeof(IFeatureFilter))]</c>, не трогая ядро.
/// <para>Реализации обязаны быть thread-safe и stateless (регистрируются Singleton).</para>
/// </summary>
public interface IFeatureFilter
{
    /// <summary>Имя фильтра — совпадает с <c>TargetingRule.FilterName</c>.</summary>
    string Name { get; }

    /// <summary>Срабатывает ли правило для данного субъекта.</summary>
    ValueTask<bool> EvaluateAsync(FeatureFilterContext context, CancellationToken ct);
}

/// <summary>Вход фильтра: ключ флага, субъект и параметры правила (из <c>TargetingRule.Parameters</c>).</summary>
public sealed record FeatureFilterContext(
    string FeatureKey,
    FeatureContext Subject,
    IReadOnlyDictionary<string, object?> Parameters);
