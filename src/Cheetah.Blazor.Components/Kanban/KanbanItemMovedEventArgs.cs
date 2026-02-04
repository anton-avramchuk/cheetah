namespace Cheetah.Blazor.Components.Kanban;

/// <summary>
/// Event arguments for when an item is moved between Kanban columns
/// </summary>
/// <typeparam name="TItem">Type of the item</typeparam>
public class KanbanItemMovedEventArgs<TItem>
{
    /// <summary>
    /// The item that was moved
    /// </summary>
    public TItem Item { get; set; } = default!;

    /// <summary>
    /// ID of the source column
    /// </summary>
    public string SourceColumnId { get; set; } = "";

    /// <summary>
    /// Index of item in source column before move
    /// </summary>
    public int SourceIndex { get; set; }

    /// <summary>
    /// ID of the target column
    /// </summary>
    public string TargetColumnId { get; set; } = "";

    /// <summary>
    /// Target index where item should be inserted
    /// </summary>
    public int TargetIndex { get; set; }

    /// <summary>
    /// Whether the item moved to a different column
    /// </summary>
    public bool ColumnChanged => SourceColumnId != TargetColumnId;
}
