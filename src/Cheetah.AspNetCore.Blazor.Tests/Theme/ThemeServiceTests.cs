using Cheetah.AspNetCore.Blazor.Theme;
using Microsoft.Extensions.Options;

namespace Cheetah.AspNetCore.Blazor.Tests.Theme;

public class ThemeServiceTests
{
    private static ThemeService Create(ThemeOptions options)
        => new(Options.Create(options));

    [Fact]
    public void BuildCss_EmitsBootstrapVariables_ForDefaultPrimary()
    {
        var css = Create(new ThemeOptions()).BuildCss();

        css.ShouldContain(":root {");
        css.ShouldContain("--bs-primary: #6366f1;");
        css.ShouldContain("--bs-primary-rgb: 99, 102, 241;");
        css.ShouldContain(".btn-primary {");
        css.ShouldContain(".btn-outline-primary {");
    }

    [Fact]
    public void BuildCss_EmitsLinkVariables_OnlyForPrimary()
    {
        var css = Create(new ThemeOptions()).BuildCss();

        css.ShouldContain("--bs-link-color: #6366f1;");
        css.ShouldContain("--bs-link-hover-color:");
    }

    [Fact]
    public void BuildCss_ReflectsCustomColors_NormalisedToLowercaseHex()
    {
        var css = Create(new ThemeOptions { Primary = "#000000", Danger = "#ABCDEF" }).BuildCss();

        css.ShouldContain("--bs-primary: #000000;");
        css.ShouldContain("--bs-danger: #abcdef;");
        css.ShouldContain("--bs-danger-rgb: 171, 205, 239;");
    }

    [Theory]
    [InlineData("primary")]
    [InlineData("success")]
    [InlineData("danger")]
    [InlineData("warning")]
    [InlineData("info")]
    public void BuildCss_ProducesRootAndButtonBlocks_ForEveryVariant(string variant)
    {
        var css = Create(new ThemeOptions()).BuildCss();

        css.ShouldContain($"--bs-{variant}:");
        css.ShouldContain($".btn-{variant} {{");
        css.ShouldContain($".btn-outline-{variant} {{");
    }
}
