namespace Cheetah.AspNetCore.Blazor.Grid;

[AttributeUsage(AttributeTargets.Property)]
public sealed class GridColumnAttribute(string label, bool show = true, int order = 0) : Attribute
{
    public string Label { get; } = label;
    public bool Show { get; } = show;
    public int Order { get; } = order;
}
