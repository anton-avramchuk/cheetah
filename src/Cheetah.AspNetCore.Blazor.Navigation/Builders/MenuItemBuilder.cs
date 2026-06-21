using Cheetah.AspNetCore.Blazor.Navigation.Models;

namespace Cheetah.AspNetCore.Blazor.Navigation.Builders;

public class MenuItemBuilder
{
    private readonly MenuItem _item;

    public MenuItemBuilder(string id, string label)
    {
        _item = new MenuItem { Id = id, Label = label };
    }

    public MenuItemBuilder WithIcon(string icon)
    {
        _item.Icon = icon;
        return this;
    }

    public MenuItemBuilder WithUrl(string url)
    {
        _item.Url = url;
        return this;
    }

    public MenuItemBuilder WithOrder(int order)
    {
        _item.Order = order;
        return this;
    }

    public MenuItemBuilder RequirePermission(string permission)
    {
        _item.RequiredPermission = permission;
        return this;
    }

    public MenuItemBuilder AddChild(string id, string label, Action<MenuItemBuilder> configure)
    {
        var child = new MenuItemBuilder(id, label);
        configure(child);
        _item.Children.Add(child.Build());
        return this;
    }

    internal MenuItem Build()
    {
        _item.Children = _item.Children.OrderBy(x => x.Order).ToList();
        return _item;
    }
}
