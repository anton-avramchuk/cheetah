using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Cheetah.DistributedLock.Postgres;

/// <summary>
/// Distributed lock через Postgres pg_try_advisory_lock. Корректнее RedLock с точки зрения
/// CAP: advisory lock привязан к сессии, при разрыве connection блокировка автоматически
/// освобождается на сервере (нет split-brain как у Redis при failover'е).
///
/// Цена: каждый удерживаемый lock = одна открытая connection к Postgres. Не злоупотребляйте
/// долгими блокировками — для leader election один lock per leader, ОК.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IDistributedLockProvider))]
public sealed class PostgresDistributedLockProvider : IDistributedLockProvider
{
    private readonly PostgresLockOptions _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PostgresDistributedLockProvider> _logger;

    public PostgresDistributedLockProvider(
        IOptions<PostgresLockOptions> options,
        IConfiguration configuration,
        ILogger<PostgresDistributedLockProvider> logger)
    {
        _options = options.Value;
        _configuration = configuration;
        _logger = logger;
    }

    public async ValueTask<IDistributedLock?> TryAcquireAsync(string key, CancellationToken cancellationToken = default)
    {
        var hashedKey = HashKey(key);
        var conn = new NpgsqlConnection(ResolveConnectionString());

        try
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var cmd = new NpgsqlCommand("SELECT pg_try_advisory_lock(@key)", conn);
            cmd.Parameters.AddWithValue("key", hashedKey);
            var acquired = (bool)(await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false))!;

            if (!acquired)
            {
                await conn.DisposeAsync().ConfigureAwait(false);
                return null;
            }

            _logger.LogDebug("Acquired pg_advisory_lock for '{Key}' (hash={Hash})", key, hashedKey);
            return new PostgresLockHandle(conn, hashedKey, key, _logger);
        }
        catch
        {
            await conn.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async ValueTask<IDistributedLock> AcquireAsync(string key, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (true)
        {
            var lockHandle = await TryAcquireAsync(key, cancellationToken).ConfigureAwait(false);
            if (lockHandle is not null) return lockHandle;

            if (DateTimeOffset.UtcNow >= deadline)
                throw new DistributedLockTimeoutException(key, timeout);

            try
            {
                await Task.Delay(_options.RetryInterval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Стабильный 64-битный хеш для имени блокировки. SHA256 первые 8 байт — низкая вероятность
    /// коллизий (2^-32 для пары случайных ключей), хорошо распределён.
    /// </summary>
    private static long HashKey(string key)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(key), hash);
        return BinaryPrimitives.ReadInt64BigEndian(hash);
    }

    private string ResolveConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
            return _options.ConnectionString;

        var cs = _configuration.GetConnectionString(_options.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException(
                $"PostgresDistributedLockProvider: ни ConnectionString, ни ConnectionStrings:{_options.ConnectionStringName} не заданы.");
        return cs;
    }

    private sealed class PostgresLockHandle : IDistributedLock
    {
        private readonly NpgsqlConnection _connection;
        private readonly long _hashedKey;
        private readonly ILogger _logger;
        private int _disposed;

        public string Key { get; }

        public PostgresLockHandle(NpgsqlConnection conn, long hashedKey, string key, ILogger logger)
        {
            _connection = conn;
            _hashedKey = hashedKey;
            _logger = logger;
            Key = key;
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

            try
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    await using var cmd = new NpgsqlCommand("SELECT pg_advisory_unlock(@key)", _connection);
                    cmd.Parameters.AddWithValue("key", _hashedKey);
                    await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // Connection-close сам по себе освободит lock на сервере, так что это не критично.
                _logger.LogWarning(ex, "Failed to explicitly unlock '{Key}' (connection-close will release)", Key);
            }
            finally
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
