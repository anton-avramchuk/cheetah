namespace Cheetah.AspNetCore.Blazor.Controls;

/// <summary>
/// Классифицирует произвольную строку-статус в семантический вариант пилюли
/// (success / danger / warning / info / secondary). Используется <see cref="CrmStatusBadge"/>
/// и страницами-списками, чтобы единообразно раскрашивать статусы без ручного маппинга.
/// Логика чистая (без Blazor) и покрыта unit-тестами.
/// </summary>
public static class StatusTone
{
    // Порядок проверки важен: «danger» идёт до «success», иначе «неактивна» поймает «актив».
    private static readonly (string Variant, string[] Keys)[] Rules =
    [
        ("danger",  ["отказ", "отклон", "отмен", "закрыт", "неактив", "архив", "ошиб", "провал",
                     "reject", "declin", "cancel", "closed", "inactive", "archiv", "error", "failed"]),
        ("warning", ["приостан", "пауза", "на паузе", "ожид", "просроч", "застоп",
                     "pending", "paused", "hold", "overdue", "warning"]),
        ("success", ["актив", "открыт", "нанят", "принят", "оффер", "готов", "заверш", "успеш", "онлайн",
                     "active", "open", "hired", "accepted", "done", "complete", "success", "online"]),
        ("info",    ["новый", "скрининг", "интервью", "черновик", "в работе",
                     "new", "screening", "interview", "draft", "in progress"]),
    ];

    /// <summary>Возвращает вариант пилюли для строки-статуса; для пустой/неизвестной — «secondary».</summary>
    public static string Classify(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return "secondary";

        var s = status.Trim().ToLowerInvariant();
        foreach (var (variant, keys) in Rules)
            foreach (var key in keys)
                if (s.Contains(key, StringComparison.Ordinal))
                    return variant;

        return "secondary";
    }

    /// <summary>Вариант и подпись для булева флага (например колонка «Активна»).</summary>
    public static (string Variant, string Label) ForBool(bool value)
        => value ? ("success", "Да") : ("secondary", "Нет");
}
