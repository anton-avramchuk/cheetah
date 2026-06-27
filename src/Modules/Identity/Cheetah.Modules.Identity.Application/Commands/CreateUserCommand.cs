using Cheetah.Core.CQRS;

namespace Cheetah.Modules.Identity.Application.Commands;

/// <summary>
/// Базовая команда создания пользователя. Хост наследует её конкретной командой
/// (можно добавить поля) и регистрирует через <c>AddCrmIdentity(...).WithUsers&lt;...&gt;()</c>.
/// </summary>
public abstract record CreateUserCommand(string UserName, string Email, string Password, IReadOnlyList<Guid>? RoleIds = null) : ICommand<Guid>;
