using System.Text.RegularExpressions;

namespace Cheetah.Core.Domain.ValueObjects;

/// <summary>
/// Номер WhatsApp. По форме это телефон, поэтому и приводится как телефон — ведущий «+» и цифры,
/// 7..15 знаков, — но живёт отдельным типом от <see cref="Phone"/> намеренно: это разные способы
/// связи. У человека бывает рабочий телефон, по которому WhatsApp не отвечает, и наоборот; сложив их
/// в одно поле, мы бы обещали интерфейсу кнопку, которой некуда вести.
///
/// Приведение снимает пробелы, дефисы и скобки — и заодно адрес страницы «wa.me/79001234567»,
/// который копируют из приглашения чаще, чем сам номер.
/// </summary>
public partial class WhatsApp : ValueObject
{
    public string Value { get; }

    private WhatsApp(string value) => Value = value;

    public static WhatsApp Create(string whatsApp)
    {
        if (string.IsNullOrWhiteSpace(whatsApp))
            throw new ArgumentException("WhatsApp cannot be empty.", nameof(whatsApp));

        var normalized = Normalize(whatsApp);

        if (!WhatsAppRegex().IsMatch(normalized))
            throw new ArgumentException(
                $"Invalid whatsapp number: '{whatsApp}'. Expected optional '+' and 7..15 digits.",
                nameof(whatsApp));

        return new WhatsApp(normalized);
    }

    private static string Normalize(string whatsApp)
    {
        var trimmed = PageAddressRegex().Replace(whatsApp.Trim(), string.Empty);

        var hasPlus = trimmed.StartsWith('+');
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());

        return hasPlus ? "+" + digits : digits;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(WhatsApp whatsApp) => whatsApp.Value;

    [GeneratedRegex(@"^\+?[1-9][0-9]{6,14}$", RegexOptions.Compiled)]
    private static partial Regex WhatsAppRegex();

    [GeneratedRegex(@"^(https?://)?(wa\.me/|api\.whatsapp\.com/send\?phone=)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex PageAddressRegex();
}
