using System.Security.Claims;
using Cheetah.AspNetCore.Blazor.Navigation.Models;

namespace Cheetah.AspNetCore.Blazor.Navigation;

/// <summary>
/// Правила видимости пункта меню: право (<see cref="IMenuAccessEvaluator"/>) И фича
/// (<see cref="IMenuFeatureEvaluator"/>). Вынесено из <c>NavMenu</c> отдельно, чтобы правила
/// покрывались юнит-тестами и переиспользовались (палитра команд, хабы).
/// </summary>
public static class MenuVisibility
{
    /// <summary>
    /// Спрашивает у эвалуатора состояние каждой РАЗЛИЧНОЙ фичи, встреченной в меню (включая вложенные
    /// пункты), и возвращает множество выключенных. Один проход на построение меню — дальше
    /// <see cref="CanSee"/> работает синхронно и не ходит по сети.
    /// </summary>
    public static async Task<IReadOnlySet<string>> CollectDisabledAsync(
        Menu menu, IMenuFeatureEvaluator evaluator, CancellationToken ct = default)
    {
        var required = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var section in menu.Sections)
            foreach (var item in section.Items)
                CollectFeatures(item, required);

        var disabled = new HashSet<string>(StringComparer.Ordinal);
        foreach (var feature in required)
            if (!await evaluator.IsEnabledAsync(feature, ct).ConfigureAwait(false))
                disabled.Add(feature);

        return disabled;
    }

    /// <summary>
    /// Пункт виден, если нет требований либо они выполнены. Группа дополнительно требует хотя бы
    /// один видимый дочерний пункт — но выключенная фича гасит группу целиком, вместе с детьми.
    /// </summary>
    public static bool CanSee(
        MenuItem item, ClaimsPrincipal user, IMenuAccessEvaluator access, IReadOnlySet<string> disabledFeatures)
    {
        if (item.RequiredFeature is { } feature && disabledFeatures.Contains(feature))
            return false;

        if (!access.CanSee(user, item.RequiredPermission))
            return false;

        if (item.IsGroup)
            return item.Children.Any(child => CanSee(child, user, access, disabledFeatures));

        return true;
    }

    private static void CollectFeatures(MenuItem item, ISet<string> into)
    {
        if (item.RequiredFeature is { } feature)
            into.Add(feature);

        foreach (var child in item.Children)
            CollectFeatures(child, into);
    }
}
