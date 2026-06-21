namespace Cheetah.AspNetCore.Blazor.Theme;

internal static class ColorHelper
{
    internal static (byte R, byte G, byte B) ParseHex(string hex)
    {
        hex = hex.TrimStart('#');
        return (
            Convert.ToByte(hex[..2], 16),
            Convert.ToByte(hex[2..4], 16),
            Convert.ToByte(hex[4..6], 16)
        );
    }

    internal static string ToHex((byte R, byte G, byte B) c)
        => $"#{c.R:x2}{c.G:x2}{c.B:x2}";

    internal static string ToRgb((byte R, byte G, byte B) c)
        => $"{c.R}, {c.G}, {c.B}";

    // mix($color, black, $weight) — Bootstrap's shade-color()
    internal static (byte R, byte G, byte B) Shade((byte R, byte G, byte B) c, float weight)
        => (
            (byte)Math.Round(c.R * (1f - weight)),
            (byte)Math.Round(c.G * (1f - weight)),
            (byte)Math.Round(c.B * (1f - weight))
        );

    // mix($color, white, $weight) — Bootstrap's tint-color()
    internal static (byte R, byte G, byte B) Tint((byte R, byte G, byte B) c, float weight)
        => (
            (byte)Math.Round(c.R + (255f - c.R) * weight),
            (byte)Math.Round(c.G + (255f - c.G) * weight),
            (byte)Math.Round(c.B + (255f - c.B) * weight)
        );
}
