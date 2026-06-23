using Cheetah.Core.Domain;

namespace Cheetah.Modules.Teams.Domain.Entities;

/// <summary>
/// Участник — справочник людей, которых можно включать в команды. Конкретный (не расширяемый)
/// агрегат: хранит отображаемое имя и опциональную ссылку на пользователя
/// (<see cref="UserId"/>, модуль Identity). Состав конкретной команды — через
/// <see cref="TeamMembership"/>. Аналог <c>ProductCategory</c> в Catalog.
/// </summary>
public sealed class TeamMember : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public Guid? UserId { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private TeamMember() { } // EF

    public static TeamMember Create(string name, Guid? userId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new TeamMember
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            UserId = userId
        };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public void LinkUser(Guid? userId) => UserId = userId;
}
