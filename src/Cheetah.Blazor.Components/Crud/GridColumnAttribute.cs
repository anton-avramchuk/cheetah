namespace Cheetah.Blazor.Components.Crud;

/// <summary>
/// Attribute to configure grid column display.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class GridColumnAttribute : Attribute
{
    /// <summary>
    /// Display name for the column header.
    /// If not set, property name is used.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Column order. Lower values appear first.
    /// Default is 0.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Format string for the value (e.g., "N2", "dd.MM.yyyy").
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// CSS class for the column.
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Column width (e.g., "100px", "20%").
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Text alignment.
    /// </summary>
    public GridColumnAlignment Alignment { get; set; } = GridColumnAlignment.Left;
}

/// <summary>
/// Attribute to hide property from grid.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class GridColumnHiddenAttribute : Attribute
{
}

/// <summary>
/// Text alignment for grid column.
/// </summary>
public enum GridColumnAlignment
{
    Left,
    Center,
    Right
}
