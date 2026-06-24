using Cheetah.Core.Domain;
using Cheetah.Modules.Teams.Domain.Abstractions;

namespace Cheetah.Modules.Teams.Domain.Entities;

/// <summary>
/// Участник — справочник людей, которых можно включать в команды. Конкретный (не расширяемый)
/// агрегат. По сути — локальная реплика пользователя из модуля Identity: идентификатор участника
/// совпадает с идентификатором пользователя, а актуальность поддерживается фоновым bulk-синком.
/// Справочник целиком наполняется из Identity (<see cref="CreateFromDirectory"/>); ручного создания
/// нет. Чтобы не нагружать БД, изменения применяются ТОЛЬКО через <see cref="Apply"/> — по контентному
/// хэшу (<see cref="UserDirectoryEntry.ComputeHash"/>), без построчного сравнения.
/// </summary>
public sealed class TeamMember : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    /// <summary>Хэш последнего применённого снимка из Identity — маркер актуальности реплики.</summary>
    public string SyncHash { get; private set; } = string.Empty;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private TeamMember() { } // EF

    /// <summary>
    /// Создаёт участника-реплику из снимка пользователя Identity. Идентификатор участника совпадает
    /// с идентификатором пользователя (отдельного поля-ссылки нет), хэш фиксируется сразу. Это
    /// единственный способ завести участника — справочник целиком наполняется из Identity.
    /// </summary>
    public static TeamMember CreateFromDirectory(UserDirectoryEntry entry)
    {
        if (entry.Id == Guid.Empty)
            throw new ArgumentException("User id cannot be empty", nameof(entry));

        var member = new TeamMember { Id = entry.Id };
        member.Apply(entry);
        return member;
    }

    /// <summary>
    /// Применяет снимок из Identity, если содержимое изменилось (сравнение по хэшу). Возвращает true,
    /// если реплика была обновлена — иначе вызывающий не трогает строку в БД.
    /// </summary>
    public bool Apply(UserDirectoryEntry entry)
    {
        var hash = entry.ComputeHash();
        if (SyncHash == hash)
            return false;

        ArgumentException.ThrowIfNullOrWhiteSpace(entry.UserName);
        Name = entry.UserName.Trim();
        SyncHash = hash;
        return true;
    }
}
