using Cheetah.Core.CQRS;

namespace Cheetah.Modules.Identity.Application.Commands;

/// <summary>Базовая команда обновления пользователя (расширяется хостом).</summary>
public abstract record UpdateUserCommand(Guid Id, string UserName, string Email, IReadOnlyList<Guid>? RoleIds = null) : ICommand;
