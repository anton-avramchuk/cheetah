using Cheetah.Core.CQRS;

namespace Cheetah.Modules.Identity.Application.Commands;

public record CreateRoleCommand(string Name) : ICommand<Guid>;
