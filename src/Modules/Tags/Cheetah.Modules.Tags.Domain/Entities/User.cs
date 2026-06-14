using Cheetah.Core.Domain;
using Cheetah.Modules.Tags.Domain.Abstractions;

namespace Cheetah.Modules.Tags.Domain.Entities;

/// <summary>
/// Локальная реплика пользователя из модуля Identity. Источник истины — Identity;
/// Tags держит денормализованную копию, которую наполняет bulk-синком при старте и
/// поддерживает по доменным событиям Identity.
///
/// Изменения применяются ТОЛЬКО через <see cref="Apply"/>: детект изменений — по
/// контентному хэшу (<see cref="UserDirectoryEntry.ComputeHash"/>), а не построчным сравнением,
/// поэтому добавление новых свойств не требует правок в синке.
/// </summary>
public class User : Entity<Guid>
{
    public string UserName { get; private set; } = null!;

    /// <summary>Хэш последнего применённого снимка — маркер актуальности реплики.</summary>
    public string SyncHash { get; private set; } = null!;

    private User() { } // EF

    public static User Create(UserDirectoryEntry entry)
    {
        if (entry.Id == Guid.Empty)
            throw new ArgumentException("User id cannot be empty", nameof(entry));

        var user = new User { Id = entry.Id };
        user.Apply(entry);
        return user;
    }

    /// <summary>
    /// Применяет снимок из Identity, если содержимое изменилось (сравнение по хэшу).
    /// Возвращает true, если реплика была обновлена.
    /// </summary>
    public bool Apply(UserDirectoryEntry entry)
    {
        var hash = entry.ComputeHash();
        if (SyncHash == hash)
            return false;

        ArgumentException.ThrowIfNullOrWhiteSpace(entry.UserName);
        UserName = entry.UserName;
        SyncHash = hash;
        return true;
    }
}
