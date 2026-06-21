namespace Cheetah.AspNetCore.Blazor.Navigation.Models;

public class Menu
{
    public string Id { get; }
    public List<MenuSection> Sections { get; } = new();

    public Menu(string id)
    {
        Id = id;
    }
}
