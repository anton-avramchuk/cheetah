namespace Cheetah.AspNetCore.Blazor.Grid;

[AttributeUsage(AttributeTargets.Property)]
public sealed class GridColumnAttribute(string label, bool show = true, int order = 0, bool sortable = true) : Attribute
{
    public string Label { get; } = label;
    public bool Show { get; } = show;
    public int Order { get; } = order;

    /// <summary>Разрешена ли сортировка по этой колонке (клик по заголовку). По умолчанию true.</summary>
    public bool Sortable { get; } = sortable;
}
