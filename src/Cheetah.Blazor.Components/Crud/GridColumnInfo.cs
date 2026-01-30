using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Cheetah.Blazor.Components.Crud;

/// <summary>
/// Metadata for a grid column extracted from property reflection.
/// </summary>
public sealed class GridColumnInfo
{
    /// <summary>
    /// Property name.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Display name for column header.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Column order.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Format string for values.
    /// </summary>
    public string? Format { get; }

    /// <summary>
    /// CSS class for the column.
    /// </summary>
    public string? CssClass { get; }

    /// <summary>
    /// Column width.
    /// </summary>
    public string? Width { get; }

    /// <summary>
    /// Text alignment.
    /// </summary>
    public GridColumnAlignment Alignment { get; }

    /// <summary>
    /// Property type.
    /// </summary>
    public Type PropertyType { get; }

    /// <summary>
    /// Getter function to extract value from object.
    /// </summary>
    public Func<object, object?> GetValue { get; }

    private GridColumnInfo(PropertyInfo property, GridColumnAttribute? attr)
    {
        PropertyName = property.Name;
        PropertyType = property.PropertyType;

        // Display name priority: GridColumnAttribute > DisplayNameAttribute > DisplayAttribute > PropertyName
        DisplayName = attr?.DisplayName
            ?? property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
            ?? property.GetCustomAttribute<DisplayAttribute>()?.Name
            ?? property.Name;

        Order = attr?.Order ?? 0;
        Format = attr?.Format;
        CssClass = attr?.CssClass;
        Width = attr?.Width;
        Alignment = attr?.Alignment ?? GridColumnAlignment.Left;

        // Create getter delegate
        GetValue = obj => property.GetValue(obj);
    }

    /// <summary>
    /// Extracts column metadata from a type.
    /// </summary>
    /// <typeparam name="T">Type to extract columns from.</typeparam>
    /// <returns>List of column metadata ordered by Order property.</returns>
    public static IReadOnlyList<GridColumnInfo> FromType<T>()
    {
        return FromType(typeof(T));
    }

    /// <summary>
    /// Extracts column metadata from a type.
    /// </summary>
    /// <param name="type">Type to extract columns from.</param>
    /// <returns>List of column metadata ordered by Order property.</returns>
    public static IReadOnlyList<GridColumnInfo> FromType(Type type)
    {
        var columns = new List<GridColumnInfo>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip hidden columns
            if (property.GetCustomAttribute<GridColumnHiddenAttribute>() != null)
                continue;

            // Skip Id property (usually not displayed in grid)
            if (property.Name == nameof(IGridViewModel.Id))
                continue;

            var attr = property.GetCustomAttribute<GridColumnAttribute>();
            columns.Add(new GridColumnInfo(property, attr));
        }

        return columns.OrderBy(c => c.Order).ThenBy(c => c.PropertyName).ToList();
    }

    /// <summary>
    /// Formats the value for display.
    /// </summary>
    public string FormatValue(object? value)
    {
        if (value == null)
            return "—";

        if (Format != null && value is IFormattable formattable)
            return formattable.ToString(Format, null) ?? "—";

        return value.ToString() ?? "—";
    }

    /// <summary>
    /// Gets CSS class for alignment.
    /// </summary>
    public string GetAlignmentClass() => Alignment switch
    {
        GridColumnAlignment.Center => "text-center",
        GridColumnAlignment.Right => "text-end",
        _ => ""
    };

    /// <summary>
    /// Gets inline style for width.
    /// </summary>
    public string? GetWidthStyle() => Width != null ? $"width: {Width};" : null;
}
