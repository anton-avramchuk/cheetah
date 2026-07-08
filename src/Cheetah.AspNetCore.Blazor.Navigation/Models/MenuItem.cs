namespace Cheetah.AspNetCore.Blazor.Navigation.Models;

public class MenuItem
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public int Order { get; set; }

    /// <summary>Необязательный бейдж-счётчик у пункта (например число непрочитанных). Пусто — не рендерится.</summary>
    public string? Badge { get; set; }

    /// <summary>Если задано — пункт виден только пользователю с этим разрешением (claim типа permission) или роли admin.</summary>
    public string? RequiredPermission { get; set; }

    public List<MenuItem> Children { get; set; } = new();

    public bool IsGroup => Children.Count > 0;
}
