using Bunit;
using Cheetah.Blazor.Components.Icons;

namespace Cheetah.Blazor.Components.Tests.Icons;

public class IconTests : TestContext
{
    [Fact]
    public void ShouldRenderBootstrapIconByDefault()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "check"));

        // Assert
        var icon = cut.Find("i");
        icon.ClassList.ShouldContain("bi");
        icon.ClassList.ShouldContain("bi-check");
    }

    [Fact]
    public void ShouldRenderMaterialSymbolsOutlined()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "home")
            .Add(p => p.Set, IconSet.Material));

        // Assert
        var icon = cut.Find("span");
        icon.ClassList.ShouldContain("material-symbols-outlined");
        icon.TextContent.ShouldBe("home");
    }

    [Fact]
    public void ShouldRenderMaterialSymbolsRounded()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "settings")
            .Add(p => p.Set, IconSet.MaterialRounded));

        // Assert
        var icon = cut.Find("span");
        icon.ClassList.ShouldContain("material-symbols-rounded");
        icon.TextContent.ShouldBe("settings");
    }

    [Fact]
    public void ShouldRenderMaterialSymbolsSharp()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "favorite")
            .Add(p => p.Set, IconSet.MaterialSharp));

        // Assert
        var icon = cut.Find("span");
        icon.ClassList.ShouldContain("material-symbols-sharp");
        icon.TextContent.ShouldBe("favorite");
    }

    [Theory]
    [InlineData(IconSize.Small, "cheetah-icon-sm")]
    [InlineData(IconSize.Medium, "cheetah-icon-md")]
    [InlineData(IconSize.Large, "cheetah-icon-lg")]
    [InlineData(IconSize.XLarge, "cheetah-icon-xl")]
    public void ShouldApplySizeClass(IconSize size, string expectedClass)
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "check")
            .Add(p => p.Size, size));

        // Assert
        var icon = cut.Find("i");
        icon.ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void ShouldApplyMediumSizeByDefault()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "check"));

        // Assert
        var icon = cut.Find("i");
        icon.ClassList.ShouldContain("cheetah-icon-md");
    }

    [Fact]
    public void ShouldApplyColorStyle()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "check")
            .Add(p => p.Color, "red"));

        // Assert
        var icon = cut.Find("i");
        icon.GetAttribute("style").ShouldContain("color: red");
    }

    [Fact]
    public void ShouldNotApplyStyleWhenNoColor()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "check"));

        // Assert
        var icon = cut.Find("i");
        icon.GetAttribute("style").ShouldBeNull();
    }

    [Fact]
    public void ShouldApplySpinClass()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "arrow-repeat")
            .Add(p => p.Spin, true));

        // Assert
        var icon = cut.Find("i");
        icon.ClassList.ShouldContain("cheetah-icon-spin");
    }

    [Fact]
    public void ShouldNotApplySpinClassWhenFalse()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "check")
            .Add(p => p.Spin, false));

        // Assert
        var icon = cut.Find("i");
        icon.ClassList.ShouldNotContain("cheetah-icon-spin");
    }

    [Fact]
    public void ShouldApplyAdditionalClasses()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "check")
            .Add(p => p.Class, "my-custom-class another-class"));

        // Assert
        var icon = cut.Find("i");
        icon.ClassList.ShouldContain("my-custom-class");
        icon.ClassList.ShouldContain("another-class");
    }

    [Fact]
    public void ShouldApplyAriaLabelWhenTitleProvided()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "info-circle")
            .Add(p => p.Title, "Information"));

        // Assert
        var icon = cut.Find("i");
        icon.GetAttribute("aria-label").ShouldBe("Information");
        icon.GetAttribute("role").ShouldBe("img");
        icon.GetAttribute("aria-hidden").ShouldBeNull();
    }

    [Fact]
    public void ShouldBeHiddenFromScreenReaderWhenNoTitle()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "check"));

        // Assert
        var icon = cut.Find("i");
        icon.GetAttribute("aria-hidden").ShouldBe("true");
        icon.GetAttribute("role").ShouldBeNull();
    }

    [Fact]
    public void ShouldPassAdditionalAttributes()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "check")
            .AddUnmatched("data-testid", "my-icon")
            .AddUnmatched("id", "icon-1"));

        // Assert
        var icon = cut.Find("i");
        icon.GetAttribute("data-testid").ShouldBe("my-icon");
        icon.GetAttribute("id").ShouldBe("icon-1");
    }

    [Fact]
    public void ShouldCombineAllOptionsCorrectly()
    {
        // Act
        var cut = RenderComponent<Icon>(parameters => parameters
            .Add(p => p.Name, "star")
            .Add(p => p.Set, IconSet.Material)
            .Add(p => p.Size, IconSize.Large)
            .Add(p => p.Color, "#ffc107")
            .Add(p => p.Spin, true)
            .Add(p => p.Class, "favorite-icon")
            .Add(p => p.Title, "Favorite"));

        // Assert
        var icon = cut.Find("span");
        icon.ClassList.ShouldContain("material-symbols-outlined");
        icon.ClassList.ShouldContain("cheetah-icon-lg");
        icon.ClassList.ShouldContain("cheetah-icon-spin");
        icon.ClassList.ShouldContain("favorite-icon");
        icon.GetAttribute("style").ShouldContain("color: #ffc107");
        icon.GetAttribute("aria-label").ShouldBe("Favorite");
        icon.TextContent.ShouldBe("star");
    }
}
