using System.Security.Cryptography;
using System.Text;

namespace Cheetah.Modules.Tags.Domain.Abstractions;

/// <summary>
/// Снимок пользователя из Identity (то, что синкается в Tags-реплику).
/// При добавлении новых синкаемых свойств — добавлять их сюда И в <see cref="ComputeHash"/>.
/// </summary>
public sealed record UserDirectoryEntry(Guid Id, string UserName)
{
    // разделитель полей в канонической строке — Unit Separator (не встречается в данных)
    private const char FieldSeparator = (char)0x1F;

    /// <summary>
    /// Стабильный хэш синкаемого содержимого (без Id). Единственное место, знающее набор полей:
    /// сравнение в синке и обновление реплики идут только по хэшу, без построчных сравнений.
    /// </summary>
    public string ComputeHash()
    {
        // при добавлении свойств — включать их в этот массив
        var canonical = string.Join(FieldSeparator, new[] { UserName });
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hash);
    }
}

/// <summary>
/// Исходящий порт к модулю Identity: получить полный список пользователей для bulk-синка.
/// Реализуется в Infrastructure (адаптер над клиентом Identity).
/// </summary>
public interface IIdentityUserDirectory
{
    ValueTask<IReadOnlyList<UserDirectoryEntry>> GetAllAsync(CancellationToken ct = default);
}
