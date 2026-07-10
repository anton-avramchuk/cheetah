namespace Cheetah.AspNetCore.Blazor.Abstractions;

/// <summary>
/// Единый шов «включена ли фича» для UI: пункты меню (<c>RequiredFeature</c>) и страницы
/// (<c>[RequireFeature]</c>). Реализуется приложением поверх <c>IFeatureManager</c>, поэтому
/// Blazor-пакеты не зависят от FeatureManagement — ровно как <c>IMenuAccessEvaluator</c>
/// держит их независимыми от Identity.
/// </summary>
public interface IFeatureVisibilityEvaluator
{
    ValueTask<bool> IsEnabledAsync(string feature, CancellationToken ct = default);
}

/// <summary>
/// Заглушка по умолчанию: все фичи включены (fail-open, как у серверного <c>FeatureGateFilter</c>,
/// когда флаги не подключены). Регистрируется через <c>TryAdd</c>, поэтому хост с движком флагов
/// перекрывает её своей реализацией.
/// </summary>
public sealed class NullFeatureVisibilityEvaluator : IFeatureVisibilityEvaluator
{
    public static readonly NullFeatureVisibilityEvaluator Instance = new();

    public ValueTask<bool> IsEnabledAsync(string feature, CancellationToken ct = default)
        => ValueTask.FromResult(true);
}
