using System.Reflection;
using Cheetah.Permissions;
using Shouldly;

namespace Cheetah.Permissions.Tests;

// Тестовые декларации для ScanAssemblies — атрибут можно вешать как на класс, так и на поля/свойства.

[Permission("Test.Order.Create", "Создание заказа")]
public static class OrderPermissions
{
    [Permission("Test.Order.Cancel", "Отмена заказа")]
    public const string Cancel = "Test.Order.Cancel";
}

public class PermissionRegistryTests
{
    [Fact]
    public void Add_Stores_Descriptor()
    {
        var registry = new PermissionRegistry();
        registry.Add("Documents.Sign", "Подписать договор", "Documents");

        registry.Contains("Documents.Sign").ShouldBeTrue();
        registry.All.ShouldHaveSingleItem();
        var d = registry.All.Single();
        d.Key.ShouldBe("Documents.Sign");
        d.Description.ShouldBe("Подписать договор");
        d.Module.ShouldBe("Documents");
    }

    [Fact]
    public void Add_Same_Key_Overwrites()
    {
        var registry = new PermissionRegistry();
        registry.Add("X", "old", "Mod");
        registry.Add("X", "new", "Mod");
        registry.All.Single().Description.ShouldBe("new");
    }

    [Fact]
    public void ScanAssemblies_Finds_Class_And_Field_Attributes()
    {
        var registry = new PermissionRegistry();
        registry.ScanAssemblies(new[] { Assembly.GetExecutingAssembly() });

        registry.Contains("Test.Order.Create").ShouldBeTrue();
        registry.Contains("Test.Order.Cancel").ShouldBeTrue();
    }

    [Fact]
    public void ScanAssemblies_Uses_Attribute_Module_When_Specified()
    {
        var registry = new PermissionRegistry();
        registry.Add("explicit", "", "ExplicitModule");
        registry.All.Single().Module.ShouldBe("ExplicitModule");
    }

    [Fact]
    public void Add_Empty_Key_Throws()
    {
        var registry = new PermissionRegistry();
        Should.Throw<ArgumentException>(() => registry.Add("", "", ""));
    }
}
