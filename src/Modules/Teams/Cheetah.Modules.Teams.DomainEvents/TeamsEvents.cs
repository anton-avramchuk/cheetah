using Cheetah.Core.Events;

namespace Cheetah.Modules.Teams.DomainEvents;

/// <summary>Команда создана.</summary>
public record TeamCreatedIntegrationEvent(Guid TeamId, string Name) : EventBase;

/// <summary>Базовые поля команды изменены.</summary>
public record TeamUpdatedIntegrationEvent(Guid TeamId) : EventBase;

/// <summary>Команда деактивирована (расформирована/архив).</summary>
public record TeamDeactivatedIntegrationEvent(Guid TeamId) : EventBase;

/// <summary>Команда снова активирована.</summary>
public record TeamActivatedIntegrationEvent(Guid TeamId) : EventBase;

/// <summary>Участник добавлен в команду с ролью.</summary>
public record TeamMemberAddedIntegrationEvent(Guid TeamId, Guid MemberId, Guid RoleId) : EventBase;

/// <summary>Участник удалён из команды.</summary>
public record TeamMemberRemovedIntegrationEvent(Guid TeamId, Guid MemberId) : EventBase;

/// <summary>Роль участника в команде изменена.</summary>
public record TeamMemberRoleChangedIntegrationEvent(Guid TeamId, Guid MemberId, Guid RoleId) : EventBase;
