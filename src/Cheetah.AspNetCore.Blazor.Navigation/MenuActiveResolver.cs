namespace Cheetah.AspNetCore.Blazor.Navigation;

/// <summary>
/// Определяет, какой пункт меню активен для текущего пути, по правилу «выигрывает самый специфичный
/// (длинный) совпавший URL». Это решает проблему префиксного совпадения: при двух пунктах
/// <c>/teams</c> и <c>/teams/roles</c> на пути <c>/teams/roles</c> активен только <c>/teams/roles</c>,
/// а на <c>/teams/{id}</c> — <c>/teams</c> (а не оба сразу).
/// </summary>
public static class MenuActiveResolver
{
    /// <summary>
    /// Возвращает нормализованный URL пункта, который должен быть активен для <paramref name="relativePath"/>,
    /// либо <c>null</c>, если совпадений нет. Совпадение — по границе сегмента: <c>/team</c> не матчит
    /// <c>/teams</c>; пустой URL (домашняя) матчит только пустой путь.
    /// </summary>
    public static string? Resolve(IEnumerable<string?> candidateUrls, string relativePath)
    {
        var path = Normalize(relativePath);

        string? best = null;
        foreach (var url in candidateUrls)
        {
            var candidate = Normalize(url);
            var matches = candidate.Length == 0
                ? path.Length == 0
                : path == candidate || path.StartsWith(candidate + "/", StringComparison.Ordinal);

            if (matches && (best is null || candidate.Length > best.Length))
                best = candidate;
        }

        return best;
    }

    /// <summary>Нормализует путь/URL: отрезает query и fragment, убирает ведущие/замыкающие '/', нижний регистр.</summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var cut = value.IndexOfAny(['?', '#']);
        if (cut >= 0)
            value = value[..cut];

        return value.Trim('/').ToLowerInvariant();
    }
}
