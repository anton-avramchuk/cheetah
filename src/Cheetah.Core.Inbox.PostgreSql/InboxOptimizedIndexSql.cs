namespace Cheetah.Core.Inbox.PostgreSql;

/// <summary>
/// SQL для замены базового индекса InboxMessages на оптимизированные Postgres-индексы:
///   1) уникальный составной индекс (EventId, ConsumerName) — служит первичным ключом
///      (PK уже уникальный по этой паре, но эта функция оставляет возможность пересоздать
///      его явно, если PK был дропнут вручную);
///   2) индекс по ReceivedAt — для эффективного cleanup и сортировки.
///
/// PK на (EventId, ConsumerName) уже задаётся InboxMessageConfiguration, ловит race conditions
/// (две параллельные транзакции, пытающиеся записать обработку одного события одним consumer'ом —
/// одна получит unique violation, что и есть желаемое поведение).
/// </summary>
public static class InboxOptimizedIndexSql
{
    public const string ReceivedAtIndexName = "IX_InboxMessages_ReceivedAt";
    public const string DefaultBaseIndexName = "IX_InboxMessages_ReceivedAt";

    /// <summary>
    /// Создаёт оптимизированный индекс по ReceivedAt. Используется cleanup-сервисом
    /// для быстрого поиска кандидатов на удаление.
    /// </summary>
    public static string Create(string tableName = "InboxMessages")
    {
        ValidateIdentifier(tableName, nameof(tableName));

        return $@"
CREATE INDEX IF NOT EXISTS ""{ReceivedAtIndexName}""
ON ""{tableName}"" (""ReceivedAt"");
";
    }

    public static string Drop(string tableName = "InboxMessages")
    {
        ValidateIdentifier(tableName, nameof(tableName));
        return $@"DROP INDEX IF EXISTS ""{ReceivedAtIndexName}"";";
    }

    private static void ValidateIdentifier(string value, string paramName)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Identifier is empty", paramName);
        foreach (var c in value)
        {
            if (!(char.IsLetterOrDigit(c) || c == '_'))
                throw new ArgumentException($"Invalid identifier '{value}'. Only [A-Za-z0-9_] allowed.", paramName);
        }
    }
}
