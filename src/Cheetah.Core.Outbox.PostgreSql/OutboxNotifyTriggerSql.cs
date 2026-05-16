namespace Cheetah.Core.Outbox.PostgreSql;

/// <summary>
/// SQL для создания триггера, который шлёт pg_notify при INSERT в OutboxMessages.
/// Используйте в EF Core миграции: migrationBuilder.Sql(OutboxNotifyTriggerSql.Create()).
/// </summary>
public static class OutboxNotifyTriggerSql
{
    public static string Create(string channelName = "outbox_new", string tableName = "OutboxMessages")
    {
        ValidateIdentifier(channelName, nameof(channelName));
        ValidateIdentifier(tableName, nameof(tableName), allowPascal: true);

        return $@"
CREATE OR REPLACE FUNCTION ""{tableName}_notify""() RETURNS TRIGGER AS $$
BEGIN
  PERFORM pg_notify('{channelName}', NEW.""Id""::text);
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS ""{tableName}_notify_trg"" ON ""{tableName}"";

CREATE TRIGGER ""{tableName}_notify_trg""
AFTER INSERT ON ""{tableName}""
FOR EACH ROW EXECUTE FUNCTION ""{tableName}_notify""();
";
    }

    public static string Drop(string tableName = "OutboxMessages")
    {
        ValidateIdentifier(tableName, nameof(tableName), allowPascal: true);
        return $@"
DROP TRIGGER IF EXISTS ""{tableName}_notify_trg"" ON ""{tableName}"";
DROP FUNCTION IF EXISTS ""{tableName}_notify""();
";
    }

    private static void ValidateIdentifier(string value, string paramName, bool allowPascal = false)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Identifier is empty", paramName);

        foreach (var c in value)
        {
            var ok = char.IsLetterOrDigit(c) || c == '_';
            if (!ok || (!allowPascal && char.IsUpper(c)))
            {
                throw new ArgumentException(
                    $"Invalid identifier '{value}'. Only [A-Za-z0-9_] allowed.", paramName);
            }
        }
    }
}
