# CrmKanban Components

Kanban board components with drag & drop support using HTML5 Drag and Drop API.

## Components

- `CrmKanbanBoard<TItem>` - Main container
- `CrmKanbanColumn<TItem>` - Column (e.g., To Do, In Progress, Done)
- `CrmKanbanCard<TItem>` - Card (auto-created from items)

## Basic Usage

```razor
@code {
    private List<TaskItem> _todo = new() { new("Task 1"), new("Task 2") };
    private List<TaskItem> _inProgress = new() { new("Task 3") };
    private List<TaskItem> _done = new();

    private void OnItemMoved(KanbanItemMovedEventArgs<TaskItem> e)
    {
        // Get source list
        var sourceList = e.SourceColumnId switch
        {
            "todo" => _todo,
            "progress" => _inProgress,
            "done" => _done,
            _ => null
        };

        // Get target list
        var targetList = e.TargetColumnId switch
        {
            "todo" => _todo,
            "progress" => _inProgress,
            "done" => _done,
            _ => null
        };

        if (sourceList == null || targetList == null) return;

        // Remove from source
        sourceList.Remove(e.Item);

        // Insert at target index
        var insertIndex = Math.Min(e.TargetIndex, targetList.Count);
        targetList.Insert(insertIndex, e.Item);
    }
}

<CrmKanbanBoard TItem="TaskItem" OnItemMoved="OnItemMoved">
    <CrmKanbanColumn TItem="TaskItem"
                     Id="todo"
                     Title="To Do"
                     Icon="list-task"
                     Items="_todo"
                     HeaderVariant="ColorVariant.Secondary" />
    <CrmKanbanColumn TItem="TaskItem"
                     Id="progress"
                     Title="In Progress"
                     Icon="arrow-repeat"
                     Items="_inProgress"
                     HeaderVariant="ColorVariant.Primary" />
    <CrmKanbanColumn TItem="TaskItem"
                     Id="done"
                     Title="Done"
                     Icon="check-circle"
                     Items="_done"
                     HeaderVariant="ColorVariant.Success" />
</CrmKanbanBoard>
```

## Custom Card Template

```razor
<CrmKanbanBoard TItem="TaskItem" OnItemMoved="OnItemMoved" OnCardClick="HandleClick">
    <CardTemplate>
        <div class="fw-bold">@context.Title</div>
        <div class="d-flex justify-content-between mt-2">
            <small class="text-muted">@context.Assignee</small>
            <span class="badge bg-info">@context.Priority</span>
        </div>
    </CardTemplate>
    <ChildContent>
        <CrmKanbanColumn TItem="TaskItem" Id="todo" Title="To Do" Items="_todo" />
        <CrmKanbanColumn TItem="TaskItem" Id="done" Title="Done" Items="_done" />
    </ChildContent>
</CrmKanbanBoard>
```

## CrmKanbanBoard Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `CardTemplate` | `RenderFragment<TItem>?` | null | Custom card rendering template |
| `MinHeight` | `string?` | "400px" | Minimum board height (null for auto) |
| `CssClass` | `string?` | null | Additional CSS classes |
| `OnItemMoved` | `EventCallback<KanbanItemMovedEventArgs<TItem>>` | - | Fired when item is dropped |
| `OnCardClick` | `EventCallback<TItem>` | - | Fired when card is clicked |

## CrmKanbanColumn Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Id` | `string` | "" | Unique column identifier |
| `Title` | `string` | "" | Column header title |
| `Icon` | `string?` | null | Bootstrap icon name |
| `Items` | `IEnumerable<TItem>` | empty | Items to display |
| `ShowCount` | `bool` | true | Show item count badge |
| `HeaderVariant` | `ColorVariant?` | null | Header background color |
| `EmptyText` | `string?` | "No items" | Text when column is empty |
| `AllowDrop` | `bool` | true | Allow dropping items |
| `HeaderTemplate` | `RenderFragment?` | null | Custom header content |
| `FooterTemplate` | `RenderFragment?` | null | Custom footer content |
| `CssClass` | `string?` | null | Additional CSS classes |

## KanbanItemMovedEventArgs

```csharp
public class KanbanItemMovedEventArgs<TItem>
{
    public TItem Item { get; set; }           // The moved item
    public string SourceColumnId { get; set; } // Source column ID
    public int SourceIndex { get; set; }       // Original position in source
    public string TargetColumnId { get; set; } // Target column ID
    public int TargetIndex { get; set; }       // Insert position in target
    public bool ColumnChanged { get; }         // True if moved to different column
}
```

## CSS Customization

Include styles via `<ComponentStyles />` or manually:

```html
<link href="_content/Cheetah.Blazor.Components/css/kanban.css" rel="stylesheet" />
```

### CSS Variables

```css
/* Override in your CSS */
.crm-kanban-column {
    --column-width: 320px;
}

.crm-kanban-card {
    /* Custom card styles */
}
```

## Notes

- Uses HTML5 Drag and Drop API (no JS libraries required)
- Cards use `@key` directive for optimized rendering
- Columns implement `IDisposable` for proper cleanup
- Drop zones between cards enable precise positioning
- Responsive: columns stack vertically on mobile
