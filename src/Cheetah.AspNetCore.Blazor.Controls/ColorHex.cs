namespace Cheetah.AspNetCore.Blazor.Controls;

/// <summary>
/// Валидация и нормализация HEX-цвета для <see cref="CrmColorPicker"/>. Чистая логика, покрыта unit-тестами.
/// </summary>
public static class ColorHex
{
    /// <summary>Полный валидный HEX вида <c>#rrggbb</c> (регистр не важен).</summary>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 7 || value[0] != '#')
            return false;

        for (var i = 1; i < 7; i++)
            if (!Uri.IsHexDigit(value[i]))
                return false;

        return true;
    }

    /// <summary>
    /// Приводит ввод к каноничному <c>#rrggbb</c> в нижнем регистре: добавляет ведущий <c>#</c> и
    /// разворачивает краткую форму <c>#rgb</c> → <c>#rrggbb</c>. Пусто → <c>null</c>. Невалидный ввод
    /// возвращается тримнутым в нижнем регистре (пользователь видит свой ввод, а валидация подсветит).
    /// </summary>
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var s = value.Trim();
        if (!s.StartsWith('#'))
            s = "#" + s;
        s = s.ToLowerInvariant();

        // Краткая форма #rgb → #rrggbb.
        if (s.Length == 4 && IsHexBody(s, 3))
            s = $"#{s[1]}{s[1]}{s[2]}{s[2]}{s[3]}{s[3]}";

        return s;
    }

    private static bool IsHexBody(string s, int len)
    {
        for (var i = 1; i <= len; i++)
            if (!Uri.IsHexDigit(s[i]))
                return false;

        return true;
    }
}
