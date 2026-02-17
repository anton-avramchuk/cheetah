using System.Text.RegularExpressions;

namespace Cheetah.Core.Domain.ValueObjects;

/// <summary>
/// Represents a hex color value in #RRGGBB or #RRGGBBAA format.
/// </summary>
public partial class Color : ValueObject
{
    public string Value { get; }

    private Color(string value) => Value = value;

    public static Color Create(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentException("Color cannot be empty.", nameof(color));

        color = color.Trim().ToLowerInvariant();

        if (!ColorRegex().IsMatch(color))
            throw new ArgumentException(
                $"Invalid color format: '{color}'. Expected #RRGGBB or #RRGGBBAA.", nameof(color));

        return new Color(color);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Color color) => color.Value;

    [GeneratedRegex(@"^#[0-9a-f]{6}([0-9a-f]{2})?$", RegexOptions.Compiled)]
    private static partial Regex ColorRegex();
}
