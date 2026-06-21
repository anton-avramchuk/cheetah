using System.Text;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.AspNetCore.Blazor.Theme;

[Export(LifetimeType.Singleton, typeof(IThemeService))]
public class ThemeService(IOptions<ThemeOptions> options) : IThemeService
{
    public string BuildCss()
    {
        var o = options.Value;
        var sb = new StringBuilder();

        sb.AppendLine(":root {");
        AppendRootVars(sb, "primary", o.Primary, isLink: true);
        AppendRootVars(sb, "success", o.Success);
        AppendRootVars(sb, "danger",  o.Danger);
        AppendRootVars(sb, "warning", o.Warning);
        AppendRootVars(sb, "info",    o.Info);
        sb.AppendLine("}");

        AppendBtnOverrides(sb, "primary", o.Primary);
        AppendBtnOverrides(sb, "success", o.Success);
        AppendBtnOverrides(sb, "danger",  o.Danger);
        AppendBtnOverrides(sb, "warning", o.Warning);
        AppendBtnOverrides(sb, "info",    o.Info);

        return sb.ToString();
    }

    private static void AppendRootVars(StringBuilder sb, string name, string hex, bool isLink = false)
    {
        var c     = ColorHelper.ParseHex(hex);
        var s60   = ColorHelper.Shade(c, 0.60f);
        var t60   = ColorHelper.Tint(c,  0.60f);
        var t80   = ColorHelper.Tint(c,  0.80f);
        var s20   = ColorHelper.Shade(c, 0.20f);

        sb.AppendLine($"    --bs-{name}: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-{name}-rgb: {ColorHelper.ToRgb(c)};");
        sb.AppendLine($"    --bs-{name}-text-emphasis: {ColorHelper.ToHex(s60)};");
        sb.AppendLine($"    --bs-{name}-bg-subtle: {ColorHelper.ToHex(t80)};");
        sb.AppendLine($"    --bs-{name}-border-subtle: {ColorHelper.ToHex(t60)};");

        if (isLink)
        {
            sb.AppendLine($"    --bs-link-color: {ColorHelper.ToHex(c)};");
            sb.AppendLine($"    --bs-link-color-rgb: {ColorHelper.ToRgb(c)};");
            sb.AppendLine($"    --bs-link-hover-color: {ColorHelper.ToHex(s20)};");
            sb.AppendLine($"    --bs-link-hover-color-rgb: {ColorHelper.ToRgb(s20)};");
        }
    }

    private static void AppendBtnOverrides(StringBuilder sb, string name, string hex)
    {
        var c   = ColorHelper.ParseHex(hex);
        var s15 = ColorHelper.Shade(c, 0.15f);
        var s20 = ColorHelper.Shade(c, 0.20f);
        var s25 = ColorHelper.Shade(c, 0.25f);

        sb.AppendLine($".btn-{name} {{");
        sb.AppendLine($"    --bs-btn-color: #fff;");
        sb.AppendLine($"    --bs-btn-bg: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-btn-border-color: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-btn-hover-color: #fff;");
        sb.AppendLine($"    --bs-btn-hover-bg: {ColorHelper.ToHex(s15)};");
        sb.AppendLine($"    --bs-btn-hover-border-color: {ColorHelper.ToHex(s20)};");
        sb.AppendLine($"    --bs-btn-focus-shadow-rgb: {ColorHelper.ToRgb(c)};");
        sb.AppendLine($"    --bs-btn-active-color: #fff;");
        sb.AppendLine($"    --bs-btn-active-bg: {ColorHelper.ToHex(s20)};");
        sb.AppendLine($"    --bs-btn-active-border-color: {ColorHelper.ToHex(s25)};");
        sb.AppendLine($"    --bs-btn-disabled-color: #fff;");
        sb.AppendLine($"    --bs-btn-disabled-bg: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-btn-disabled-border-color: {ColorHelper.ToHex(c)};");
        sb.AppendLine("}");

        sb.AppendLine($".btn-outline-{name} {{");
        sb.AppendLine($"    --bs-btn-color: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-btn-border-color: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-btn-hover-color: #fff;");
        sb.AppendLine($"    --bs-btn-hover-bg: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-btn-hover-border-color: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-btn-focus-shadow-rgb: {ColorHelper.ToRgb(c)};");
        sb.AppendLine($"    --bs-btn-active-color: #fff;");
        sb.AppendLine($"    --bs-btn-active-bg: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-btn-active-border-color: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-btn-disabled-color: {ColorHelper.ToHex(c)};");
        sb.AppendLine($"    --bs-btn-disabled-bg: transparent;");
        sb.AppendLine($"    --bs-btn-disabled-border-color: {ColorHelper.ToHex(c)};");
        sb.AppendLine("}");
    }
}
