using Cheetah.Core.CQRS;

namespace Cheetah.Modules.Identity.Application.Commands;

/// <summary>Базовая команда обновления роли (расширяется хостом).</summary>
public abstract record UpdateRoleCommand(Guid Id, string Name) : ICommand;
