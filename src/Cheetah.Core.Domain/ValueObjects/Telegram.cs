using System.Text.RegularExpressions;

namespace Cheetah.Core.Domain.ValueObjects;

/// <summary>
/// Telegram username. Хранится каноническим: без «@», без адреса страницы и в нижнем регистре.
///
/// Приведение обязательно, потому что одно и то же имя приходит четырьмя видами — «@user», «user»,
/// «t.me/user», «https://telegram.me/user», — и без него в базе окажутся четыре разных контакта одного
/// человека, а поиск по нику не найдёт ни одного. Регистр не значим: Telegram сам не различает
/// «User» и «user», и хранить два варианта значило бы уметь то, чего не умеет мессенджер.
///
/// Ссылку «написать» строит интерфейс: она однозначно выводится из имени, и дублировать её в домене
/// незачем.
/// </summary>
public partial class Telegram : ValueObject
{
    public string Value { get; }

    private Telegram(string value) => Value = value;

    public static Telegram Create(string telegram)
    {
        if (string.IsNullOrWhiteSpace(telegram))
            throw new ArgumentException("Telegram cannot be empty.", nameof(telegram));

        var normalized = Normalize(telegram);

        if (!TelegramRegex().IsMatch(normalized))
            throw new ArgumentException(
                $"Invalid telegram username: '{telegram}'. Expected 5..32 characters: latin letters, "
                + "digits and underscore, starting with a letter.",
                nameof(telegram));

        return new Telegram(normalized);
    }

    /// <summary>Снимает «@» и адрес страницы, приводит к нижнему регистру.</summary>
    private static string Normalize(string telegram)
    {
        var value = telegram.Trim();

        // Адрес страницы — то, что чаще всего копируют из профиля.
        value = PageAddressRegex().Replace(value, string.Empty);

        return value.TrimStart('@').Trim('/').ToLowerInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Telegram telegram) => telegram.Value;

    [GeneratedRegex(@"^[a-z][a-z0-9_]{4,31}$", RegexOptions.Compiled)]
    private static partial Regex TelegramRegex();

    [GeneratedRegex(@"^(https?://)?(t\.me|telegram\.me)/", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex PageAddressRegex();
}
