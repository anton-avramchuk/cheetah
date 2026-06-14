using System.Text.RegularExpressions;

namespace Cheetah.Core.Domain.ValueObjects;

/// <summary>
/// Represents a phone number in E.164-like format (optional leading '+', 7..15 digits).
/// </summary>
public partial class Phone : ValueObject
{
    public string Value { get; }

    private Phone(string value) => Value = value;

    public static Phone Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty.", nameof(phone));

        var normalized = Normalize(phone);

        if (!PhoneRegex().IsMatch(normalized))
            throw new ArgumentException(
                $"Invalid phone format: '{phone}'. Expected optional '+' and 7..15 digits.", nameof(phone));

        return new Phone(normalized);
    }

    /// <summary>Удаляет пробелы, дефисы и скобки, сохраняя ведущий '+'.</summary>
    private static string Normalize(string phone)
    {
        var trimmed = phone.Trim();
        var hasPlus = trimmed.StartsWith('+');
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        return hasPlus ? "+" + digits : digits;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Phone phone) => phone.Value;

    [GeneratedRegex(@"^\+?[1-9][0-9]{6,14}$", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();
}
