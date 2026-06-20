using System.Security.Cryptography;
using System.Text;

namespace Cheetah.FeatureManagement;

/// <summary>
/// Детерминированный хэш для стабильного percentage-rollout и распределения вариантов.
/// НЕ использовать <see cref="string.GetHashCode()"/> — он рандомизирован per-process и дал бы
/// «мигание» флага между инстансами/запросами.
/// </summary>
internal static class StableHash
{
    /// <summary>Возвращает стабильное значение в диапазоне [0, 100).</summary>
    public static int Bucket(string value)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(value), hash);
        // Берём первые 8 байт как беззнаковое число и берём модуль 100.
        ulong n = BitConverter.ToUInt64(hash[..8]);
        return (int)(n % 100UL);
    }
}
