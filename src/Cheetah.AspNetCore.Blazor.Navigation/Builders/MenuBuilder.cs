using Cheetah.AspNetCore.Blazor.Navigation.Models;

namespace Cheetah.AspNetCore.Blazor.Navigation.Builders;

public class MenuBuilder
{
    private readonly string _menuId;
    private readonly List<MenuSectionBuilder> _sectionBuilders = new();

    public string MenuId => _menuId;

    public MenuBuilder(string menuId)
    {
        _menuId = menuId;
    }

    public MenuSectionBuilder AddSection(string id, string label, int order = 0)
    {
        var existing = _sectionBuilders.FirstOrDefault(s => s.Id == id);
        if (existing != null)
            return existing;

        var builder = new MenuSectionBuilder(id, label, order);
        _sectionBuilders.Add(builder);
        return builder;
    }

    internal Menu Build()
    {
        var menu = new Menu(_menuId);
        menu.Sections.AddRange(_sectionBuilders.Select(b => b.Build()).OrderBy(s => s.Order));
        return menu;
    }
}
