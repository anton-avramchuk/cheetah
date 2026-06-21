using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;

namespace Cheetah.AspNetCore.Blazor.Tests.Navigation;

public class NavigationMenuServiceTests
{
    private sealed class FakeContributor(string targetMenuId, Action<MenuBuilder> configure) : IMenuContributor
    {
        public string TargetMenuId { get; } = targetMenuId;

        public Task ConfigureMenuAsync(MenuBuilder builder)
        {
            configure(builder);
            return Task.CompletedTask;
        }
    }

    private static NavigationMenuService ServiceWith(params IMenuContributor[] contributors)
        => new(new MenuContributorProvider(contributors));

    [Fact]
    public async Task GetMenuAsync_AppliesOnlyContributorsForRequestedMenu()
    {
        var service = ServiceWith(
            new FakeContributor("main", b => b.AddSection("s-main", "Main")),
            new FakeContributor("other", b => b.AddSection("s-other", "Other")));

        var menu = await service.GetMenuAsync("main");

        menu.Id.ShouldBe("main");
        menu.Sections.Select(s => s.Id).ShouldBe(["s-main"]);
    }

    [Fact]
    public async Task GetMenuAsync_SortsSectionsByOrder()
    {
        var service = ServiceWith(new FakeContributor("main", b =>
        {
            b.AddSection("second", "Second", order: 2);
            b.AddSection("first", "First", order: 1);
        }));

        var menu = await service.GetMenuAsync("main");

        menu.Sections.Select(s => s.Id).ShouldBe(["first", "second"]);
    }

    [Fact]
    public async Task GetMenuAsync_SortsItemsByOrder_AndCarriesItemProperties()
    {
        var service = ServiceWith(new FakeContributor("main", b =>
        {
            var section = b.AddSection("data", "Data");
            section.AddItem("b", "B").WithOrder(2).WithUrl("b");
            section.AddItem("a", "A").WithOrder(1).WithIcon("bi bi-gear").WithUrl("a").RequirePermission("perm.a");
        }));

        var items = (await service.GetMenuAsync("main")).Sections.Single().Items;

        items.Select(i => i.Id).ShouldBe(["a", "b"]);
        items[0].Icon.ShouldBe("bi bi-gear");
        items[0].Url.ShouldBe("a");
        items[0].RequiredPermission.ShouldBe("perm.a");
    }

    [Fact]
    public async Task GetMenuAsync_AddChild_TurnsItemIntoSortedGroup()
    {
        var service = ServiceWith(new FakeContributor("main", b =>
        {
            b.AddSection("data", "Data")
                .AddItem("group", "Group")
                .AddChild("c2", "Child2", c => c.WithOrder(2).WithUrl("c2"))
                .AddChild("c1", "Child1", c => c.WithOrder(1).WithUrl("c1"));
        }));

        var group = (await service.GetMenuAsync("main")).Sections.Single().Items.Single();

        group.IsGroup.ShouldBeTrue();
        group.Children.Select(c => c.Id).ShouldBe(["c1", "c2"]);
    }

    [Fact]
    public async Task GetMenuAsync_MergesContributorsIntoSameSectionId()
    {
        var service = ServiceWith(
            new FakeContributor("main", b => b.AddSection("shared", "Shared").AddItem("a", "A")),
            new FakeContributor("main", b => b.AddSection("shared", "Shared").AddItem("b", "B")));

        var menu = await service.GetMenuAsync("main");

        menu.Sections.Count.ShouldBe(1);
        menu.Sections.Single().Items.Select(i => i.Id).ShouldBe(["a", "b"], ignoreOrder: true);
    }
}
