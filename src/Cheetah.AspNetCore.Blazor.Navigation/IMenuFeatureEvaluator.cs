namespace Cheetah.AspNetCore.Blazor.Navigation;

/// <summary>
/// Решает, включена ли фича, за которой спрятан пункт меню (<see cref="Models.MenuItem.RequiredFeature"/>).
/// Реализуется хостом поверх <c>IFeatureManager</c>; Navigation остаётся независимым от FeatureManagement —
/// ровно как <see cref="IMenuAccessEvaluator"/> держит его независимым от Identity.
/// </summary>
public interface IMenuFeatureEvaluator
{
    ValueTask<bool> IsEnabledAsync(string feature, CancellationToken ct = default);
}

/// <summary>
/// Заглушка по умолчанию: все фичи включены. Регистрируется через <c>TryAdd</c>, поэтому хост,
/// подключивший движок фич-флагов, перекрывает её своей реализацией.
/// </summary>
public sealed class NullMenuFeatureEvaluator : IMenuFeatureEvaluator
{
    public static readonly NullMenuFeatureEvaluator Instance = new();

    public ValueTask<bool> IsEnabledAsync(string feature, CancellationToken ct = default)
        => ValueTask.FromResult(true);
}
