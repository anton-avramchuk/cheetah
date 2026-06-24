using System.Security.Cryptography;
using System.Text;

namespace Cheetah.Modules.Teams.Domain.Abstractions;

/// <summary>
/// Снимок пользователя из Identity (то, что синкается в реплику-участника Teams). Идентификатор
/// участника совпадает с идентификатором пользователя Identity. При добавлении новых синкаемых
/// свойств — добавлять их сюда И в <see cref="ComputeHash"/>.
/// </summary>
public sealed record UserDirectoryEntry(Guid Id, string UserName)
{
    // разделитель полей в канонической строке — Unit Separator (не встречается в данных)
    private const char FieldSeparator = (char)0x1F;

    /// <summary>
    /// Стабильный хэш синкаемого содержимого (без Id). Единственное место, знающее набор полей:
    /// сравнение в синке и обновление реплики идут только по хэшу, без построчных сравнений, поэтому
    /// фоновый синк не переписывает строки, чьё содержимое не изменилось.
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
/// Исходящий порт к модулю Identity: получить полный список пользователей для bulk-синка реплики
/// участников. Реализуется в Infrastructure (адаптер над клиентом Identity).
/// </summary>
public interface IIdentityUserDirectory
{
    ValueTask<IReadOnlyList<UserDirectoryEntry>> GetAllAsync(CancellationToken ct = default);
}

/// <summary>
/// Оркестратор полного синка реплики участников из Identity (тянет снимки через
/// <see cref="IIdentityUserDirectory"/> и апсёртит их в справочник, обновляя только изменившиеся по
/// хэшу записи). Реализуется в Application; вызывается фоновым сервисом Infrastructure. Возвращает
/// число фактически изменённых записей.
/// </summary>
public interface ITeamMemberDirectorySynchronizer
{
    ValueTask<int> SyncAsync(CancellationToken ct = default);
}
