using Cheetah.Core.Domain;

namespace Cheetah.Modules.Teams.Domain.Entities;

/// <summary>
/// Роль участника в команде (например «Лид», «Менеджер», «Наблюдатель») — конкретный
/// (не расширяемый) справочный агрегат: набор ролей редко требует доменного расширения. Аналог
/// <c>ProductCategory</c> в Catalog.
/// </summary>
public sealed class TeamRole : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private TeamRole() { } // EF

    public static TeamRole Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new TeamRole
        {
            Id = Guid.NewGuid(),
            Name = name.Trim()
        };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }
}
