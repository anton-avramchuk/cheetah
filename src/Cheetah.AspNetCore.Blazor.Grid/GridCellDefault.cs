using System.Globalization;

namespace Cheetah.AspNetCore.Blazor.Grid;

/// <summary>Как отрисовать значение ячейки грида по умолчанию (без явного шаблона).</summary>
public enum GridCellKind
{
    /// <summary>Обычный текст.</summary>
    Text,
    /// <summary>Приглушённый плейсхолдер (пустое значение → «—»).</summary>
    Muted,
    /// <summary>Булево «истина» → пилюля-успех.</summary>
    BoolTrue,
    /// <summary>Булево «ложь» → нейтральная пилюля.</summary>
    BoolFalse,
    /// <summary>Дата/время в человекочитаемом формате.</summary>
    Date,
}

/// <summary>
/// Дефолтный форматтер ячейки грида: заменяет «сырой» <c>value.ToString()</c> на осмысленную
/// подачу — булево становится пилюлей (чинит «True/False»), пустое — приглушённым «—», даты
/// форматируются. Логика чистая и покрыта unit-тестами; рендер — в CrmGrid.razor.
/// </summary>
public static class GridCellDefault
{
    /// <summary>Определяет вид и текст ячейки для произвольного значения.</summary>
    public static (GridCellKind Kind, string Text) Describe(object? value)
        => value switch
        {
            null              => (GridCellKind.Muted, "—"),
            bool b            => b ? (GridCellKind.BoolTrue, "Да") : (GridCellKind.BoolFalse, "Нет"),
            string s          => string.IsNullOrWhiteSpace(s) ? (GridCellKind.Muted, "—") : (GridCellKind.Text, s),
            DateTime dt       => (GridCellKind.Date, FormatDateTime(dt)),
            DateTimeOffset dto => (GridCellKind.Date, FormatDateTime(dto.DateTime)),
            DateOnly d        => (GridCellKind.Date, d.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)),
            _                 => Fallback(value),
        };

    private static string FormatDateTime(DateTime dt)
        => dt.TimeOfDay == TimeSpan.Zero
            ? dt.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
            : dt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

    private static (GridCellKind, string) Fallback(object value)
    {
        var s = value.ToString();
        return string.IsNullOrWhiteSpace(s) ? (GridCellKind.Muted, "—") : (GridCellKind.Text, s);
    }
}
