using Cheetah.Core.CQRS;

namespace Cheetah.Modules.Identity.Application.Commands;

public record DeleteRoleCommand(Guid Id) : ICommand;
