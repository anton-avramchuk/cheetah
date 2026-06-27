using Cheetah.Core.CQRS;

namespace Cheetah.Modules.Identity.Application.Commands;

/// <summary>Базовая команда создания роли (расширяется хостом).</summary>
public abstract record CreateRoleCommand(string Name) : ICommand<Guid>;
