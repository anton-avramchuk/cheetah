using Cheetah.AspNetCore.Blazor.Navigation.Models;

namespace Cheetah.AspNetCore.Blazor.Navigation.Builders;

public class MenuSectionBuilder
{
    private readonly MenuSection _section;
    private readonly List<MenuItemBuilder> _itemBuilders = new();

    public string Id => _section.Id;

    public MenuSectionBuilder(string id, string label, int order)
    {
        _section = new MenuSection { Id = id, Label = label, Order = order };
    }

    public MenuItemBuilder AddItem(string id, string label)
    {
        var builder = new MenuItemBuilder(id, label);
        _itemBuilders.Add(builder);
        return builder;
    }

    internal MenuSection Build()
    {
        _section.Items.AddRange(_itemBuilders.Select(b => b.Build()).OrderBy(x => x.Order));
        return _section;
    }
}
