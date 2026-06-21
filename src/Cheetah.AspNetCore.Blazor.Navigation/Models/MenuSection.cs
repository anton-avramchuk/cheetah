namespace Cheetah.AspNetCore.Blazor.Navigation.Models;

public class MenuSection
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<MenuItem> Items { get; set; } = new();
}
