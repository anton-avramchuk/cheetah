using Cheetah.Core.Domain;
using Cheetah.Modules.Teams.DomainEvents;

namespace Cheetah.Modules.Teams.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат команды. Шаблонный модуль не инстанцирует его сам — наследник
/// объявляет конкретный <c>sealed class Team : TeamBase</c> со своей фабрикой (через
/// <see cref="InitializeCore"/>) и доп. полями (отдел, цвет, аватар…). Это и есть точка
/// расширяемости сущности.
/// <para>
/// Команда владеет составом — коллекцией <see cref="TeamMembership"/> (участник + роль). Состав
/// меняется только через методы агрегата (<see cref="AddMember"/>/<see cref="RemoveMember"/>/
/// <see cref="ChangeMemberRole"/>), что держит инвариант «один участник — одна роль в команде».
/// Расформирование — мягкое, через <see cref="IsActive"/>.
/// </para>
/// </summary>
public abstract class TeamBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<TeamMembership> _members = new();

    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public IReadOnlyList<TeamMembership> Members => _members;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected TeamBase() { } // EF + наследник

    /// <summary>
    /// Заводит инварианты новой команды и доменное событие создания. Вызывается фабрикой наследника
    /// (замена <c>new</c> абстрактной сущности).
    /// </summary>
    protected void InitializeCore(Guid id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name.Trim();
        IsActive = true;

        AddDomainEvent(new TeamCreatedIntegrationEvent(Id, Name));
    }

    /// <summary>Переименование команды. Доп. поля наследника обновляет он сам (переопределив хендлер).</summary>
    public virtual void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        AddDomainEvent(new TeamUpdatedIntegrationEvent(Id));
    }

    public virtual void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        AddDomainEvent(new TeamDeactivatedIntegrationEvent(Id));
    }

    public virtual void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        AddDomainEvent(new TeamActivatedIntegrationEvent(Id));
    }

    /// <summary>Добавляет участника с ролью. Если участник уже в команде — меняет роль (идемпотентно).</summary>
    public void AddMember(Guid memberId, Guid roleId)
    {
        var membership = _members.FirstOrDefault(m => m.MemberId == memberId);
        if (membership is null)
        {
            _members.Add(TeamMembership.Create(Id, memberId, roleId));
            AddDomainEvent(new TeamMemberAddedIntegrationEvent(Id, memberId, roleId));
        }
        else if (membership.RoleId != roleId)
        {
            membership.ChangeRole(roleId);
            AddDomainEvent(new TeamMemberRoleChangedIntegrationEvent(Id, memberId, roleId));
        }
    }

    /// <summary>Меняет роль уже состоящего участника.</summary>
    public void ChangeMemberRole(Guid memberId, Guid roleId)
    {
        var membership = _members.FirstOrDefault(m => m.MemberId == memberId)
            ?? throw new InvalidOperationException($"Member '{memberId}' is not in team '{Id}'.");

        if (membership.RoleId == roleId)
            return;

        membership.ChangeRole(roleId);
        AddDomainEvent(new TeamMemberRoleChangedIntegrationEvent(Id, memberId, roleId));
    }

    /// <summary>Удаляет участника из команды (no-op, если его нет).</summary>
    public void RemoveMember(Guid memberId)
    {
        var membership = _members.FirstOrDefault(m => m.MemberId == memberId);
        if (membership is null)
            return;

        _members.Remove(membership);
        AddDomainEvent(new TeamMemberRemovedIntegrationEvent(Id, memberId));
    }
}
