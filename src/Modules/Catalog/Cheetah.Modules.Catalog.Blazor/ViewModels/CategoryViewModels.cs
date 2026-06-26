using Cheetah.AspNetCore.Blazor.Grid;

namespace Cheetah.Modules.Catalog.Blazor.ViewModels;

/// <summary>«Лёгкий» ViewModel категории для грида. Колонки описаны атрибутами — отдельный UI не нужен.</summary>
public sealed class CategoryGridViewModel : IHasId
{
    public Guid Id { get; set; }

    [GridColumn("Название", order: 0)]
    public string Name { get; set; } = string.Empty;

    [GridColumn("Путь", order: 1)]
    public string Path { get; set; } = string.Empty;

    [GridColumn("Порядок", order: 2)]
    public int Order { get; set; }
}

/// <summary>ViewModel категории для формы редактирования (двусторонний биндинг).</summary>
public sealed class CategoryDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}

/// <summary>ViewModel категории для формы создания.</summary>
public sealed class CategoryCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}
