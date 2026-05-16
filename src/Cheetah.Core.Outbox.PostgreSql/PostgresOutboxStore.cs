using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Outbox.PostgreSql;

/// <summary>
/// Postgres-вариант EfOutboxStore: GetPendingAsync использует
/// SELECT ... FOR UPDATE SKIP LOCKED + UPDATE ... RETURNING для атомарного "захвата" сообщений.
/// Безопасно при запуске нескольких реплик: ни одна не возьмёт чужую пачку.
/// </summary>
public sealed class PostgresOutboxStore<TContext> : EfOutboxStore<TContext>
    where TContext : DbContext, IOutboxDbContext
{
    private readonly PostgresOutboxOptions _pgOptions;

    public PostgresOutboxStore(TContext context, IOptions<PostgresOutboxOptions> pgOptions) : base(context)
    {
        _pgOptions = pgOptions.Value;
    }

    public override async ValueTask<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var claimSeconds = (int)_pgOptions.ClaimTimeout.TotalSeconds;

        // Атомарный claim: SELECT ... FOR UPDATE SKIP LOCKED во вложенном запросе блокирует строки,
        // внешний UPDATE сдвигает NextAttemptAt вперёд и возвращает их. После коммита блокировки
        // снимаются, но из-за NextAttemptAt в будущем — другие реплики их не увидят до ClaimTimeout
        // или пока processor не вызовет MarkProcessedAsync.
        var sql = $@"
UPDATE ""OutboxMessages""
SET ""NextAttemptAt"" = NOW() + INTERVAL '{claimSeconds} seconds'
WHERE ""Id"" IN (
    SELECT ""Id""
    FROM ""OutboxMessages""
    WHERE ""ProcessedAt"" IS NULL
      AND (""NextAttemptAt"" IS NULL OR ""NextAttemptAt"" <= NOW())
    ORDER BY ""OccurredAt""
    LIMIT {batchSize}
    FOR UPDATE SKIP LOCKED
)
RETURNING *;";

        var list = await Context.OutboxMessages
            .FromSqlRaw(sql)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return list;
    }
}
