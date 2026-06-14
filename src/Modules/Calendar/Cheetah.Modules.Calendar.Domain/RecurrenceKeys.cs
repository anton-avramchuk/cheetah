namespace Cheetah.Modules.Calendar.Domain;

/// <summary>
/// Канонический ключ экземпляра серии (аналог iCal RECURRENCE-ID) — строковое
/// представление исходного UTC-времени начала вхождения. Используется как стабильный
/// идентификатор экземпляра в override-ах, EXDATE-сопоставлении и материализованных триггерах.
/// </summary>
public static class RecurrenceKeys
{
    private const string Format = "yyyyMMdd'T'HHmmss'Z'";

    public static string FromUtc(DateTime startUtc)
        => DateTime.SpecifyKind(startUtc, DateTimeKind.Utc).ToString(Format);

    public static bool TryParse(string key, out DateTime startUtc)
    {
        if (DateTime.TryParseExact(key, Format, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out startUtc))
        {
            startUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
            return true;
        }

        startUtc = default;
        return false;
    }
}
