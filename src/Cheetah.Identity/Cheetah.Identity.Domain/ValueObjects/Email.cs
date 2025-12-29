using System.Text.RegularExpressions;
using Cheetah.Core.Domain;

namespace Cheetah.Identity.Domain.ValueObjects;

/// <summary>
/// Email value object with validation
/// </summary>
public sealed partial class Email : ValueObject
{
    // Email regex pattern (RFC 5322 simplified)
    private static readonly Regex EmailRegex = GetEmailRegex();

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        value = value.Trim().ToLowerInvariant();

        if (value.Length > 256)
            throw new ArgumentException("Email cannot exceed 256 characters", nameof(value));

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));

        return new Email(value);
    }

    public static Email? TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return Create(value);
        }
        catch
        {
            return null;
        }
    }

    public string Normalize() => Value.ToUpperInvariant();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex GetEmailRegex();
}
