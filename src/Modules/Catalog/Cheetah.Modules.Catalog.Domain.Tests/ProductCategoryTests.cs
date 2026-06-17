using Cheetah.Modules.Catalog.Domain.Entities;
using Shouldly;

namespace Cheetah.Modules.Catalog.Domain.Tests;

public class ProductCategoryTests
{
    [Fact]
    public void Create_root_builds_path_from_own_id()
    {
        var root = ProductCategory.Create("Electronics", parent: null);

        root.ParentId.ShouldBeNull();
        root.Path.ShouldBe($"/{root.Id}");
    }

    [Fact]
    public void Create_child_appends_to_parent_path()
    {
        var root = ProductCategory.Create("Electronics", parent: null);

        var child = ProductCategory.Create("Phones", root);

        child.ParentId.ShouldBe(root.Id);
        child.Path.ShouldBe($"{root.Path}/{child.Id}");
    }

    [Fact]
    public void Create_requires_name()
        => Should.Throw<ArgumentException>(() => ProductCategory.Create(" ", parent: null));
}
