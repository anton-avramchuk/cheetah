namespace Cheetah.DistributedLock.Postgres;

public class PostgresLockOptions
{
    /// <summary>
    /// Connection string. Если null — берётся ConnectionStrings:<see cref="ConnectionStringName"/>.
    /// </summary>
    public string? ConnectionString { get; set; }

    public string ConnectionStringName { get; set; } = "DistributedLock";

    /// <summary>
    /// Интервал между retry-попытками для блокирующего AcquireAsync.
    /// (pg_advisory_lock сам блокирует на стороне сервера — у нас только timeout-loop.)
    /// </summary>
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(200);
}
