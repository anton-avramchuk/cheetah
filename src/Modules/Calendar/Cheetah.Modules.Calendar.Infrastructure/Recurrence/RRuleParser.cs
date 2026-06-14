using System.Globalization;

namespace Cheetah.Modules.Calendar.Infrastructure.Recurrence;

/// <summary>
/// Парсер поднабора RFC 5545 RRULE в структуру <see cref="Rule"/>. Неподдерживаемые части
/// (BYSETPOS, BYWEEKNO, WKST≠MO и т.п.) игнорируются — это сознательное упрощение движка.
/// </summary>
internal static class RRuleParser
{
    public enum Freq { Daily, Weekly, Monthly, Yearly }

    public sealed class Rule
    {
        public Freq Frequency { get; init; } = Freq.Daily;
        public int Interval { get; init; } = 1;
        public int? Count { get; init; }
        public DateTime? UntilUtc { get; init; }
        public List<DayOfWeek> ByDay { get; init; } = new();
        public List<int> ByMonthDay { get; init; } = new();
    }

    public static Rule Parse(string rrule)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rrule);

        var freq = Freq.Daily;
        var interval = 1;
        int? count = null;
        DateTime? until = null;
        var byDay = new List<DayOfWeek>();
        var byMonthDay = new List<int>();

        // Допускаем префикс "RRULE:".
        var body = rrule.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase) ? rrule[6..] : rrule;

        foreach (var part in body.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2)
                continue;

            var key = kv[0].Trim().ToUpperInvariant();
            var value = kv[1].Trim();

            switch (key)
            {
                case "FREQ":
                    freq = value.ToUpperInvariant() switch
                    {
                        "DAILY" => Freq.Daily,
                        "WEEKLY" => Freq.Weekly,
                        "MONTHLY" => Freq.Monthly,
                        "YEARLY" => Freq.Yearly,
                        _ => freq
                    };
                    break;

                case "INTERVAL":
                    if (int.TryParse(value, out var iv) && iv > 0)
                        interval = iv;
                    break;

                case "COUNT":
                    if (int.TryParse(value, out var c) && c > 0)
                        count = c;
                    break;

                case "UNTIL":
                    until = ParseUntil(value);
                    break;

                case "BYDAY":
                    foreach (var d in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        if (TryParseDay(d, out var dow))
                            byDay.Add(dow);
                    break;

                case "BYMONTHDAY":
                    foreach (var d in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        if (int.TryParse(d, out var md))
                            byMonthDay.Add(md);
                    break;
            }
        }

        return new Rule
        {
            Frequency = freq,
            Interval = interval,
            Count = count,
            UntilUtc = until,
            ByDay = byDay,
            ByMonthDay = byMonthDay
        };
    }

    private static DateTime? ParseUntil(string value)
    {
        string[] formats = { "yyyyMMdd'T'HHmmss'Z'", "yyyyMMdd'T'HHmmss", "yyyyMMdd" };
        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return null;
    }

    private static bool TryParseDay(string token, out DayOfWeek day)
    {
        // Отбрасываем возможный порядковый префикс (напр. "2MO") — ординалы не поддерживаем.
        var code = new string(token.Where(char.IsLetter).ToArray()).ToUpperInvariant();
        day = code switch
        {
            "MO" => DayOfWeek.Monday,
            "TU" => DayOfWeek.Tuesday,
            "WE" => DayOfWeek.Wednesday,
            "TH" => DayOfWeek.Thursday,
            "FR" => DayOfWeek.Friday,
            "SA" => DayOfWeek.Saturday,
            "SU" => DayOfWeek.Sunday,
            _ => (DayOfWeek)(-1)
        };
        return (int)day >= 0;
    }
}
