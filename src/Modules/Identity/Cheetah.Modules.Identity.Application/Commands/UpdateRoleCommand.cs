using Cheetah.Core.CQRS;

namespace Cheetah.Modules.Identity.Application.Commands;

public record UpdateRoleCommand(Guid Id, string Name) : ICommand;
