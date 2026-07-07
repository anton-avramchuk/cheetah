namespace Cheetah.AspNetCore.Blazor.Controls;

/// <summary>
/// Инициалы и детерминированный цвет фона для аватара по имени. Стабильно между запусками
/// (собственный FNV-1a хэш — НЕ <see cref="string.GetHashCode()"/>, который рандомизирован).
/// Логика чистая и покрыта unit-тестами.
/// </summary>
public static class AvatarText
{
    // Пары градиента (from, to) — приятная палитра, совпадает с прототипом редизайна.
    private static readonly (string From, string To)[] Palette =
    [
        ("#e0403f", "#ff7a6b"), ("#e08600", "#ffbe4d"), ("#16a34a", "#5fd08a"),
        ("#2f7fed", "#6db3ff"), ("#8b5cf6", "#b794ff"), ("#0ea5a4", "#5fd6d0"),
        ("#d6409f", "#f07fce"), ("#6366f1", "#9b8cff"),
    ];

    /// <summary>1–2 буквы верхним регистром: из первого и последнего слова; для пустого — «?».</summary>
    public static string Initials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";

        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return parts[0][..1].ToUpperInvariant();

        var first = parts[0][..1];
        var last = parts[^1][..1];
        return (first + last).ToUpperInvariant();
    }

    /// <summary>Индекс палитры 0..N-1, стабильный для имени (FNV-1a mod размер палитры).</summary>
    public static int ColorIndex(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return 0;

        uint hash = 2166136261;
        foreach (var c in name.Trim())
        {
            hash ^= c;
            hash *= 16777619;
        }
        return (int)(hash % (uint)Palette.Length);
    }

    /// <summary>CSS-значение фона (линейный градиент) для аватара по имени.</summary>
    public static string Background(string? name)
    {
        var (from, to) = Palette[ColorIndex(name)];
        return $"linear-gradient(135deg, {from}, {to})";
    }
}
