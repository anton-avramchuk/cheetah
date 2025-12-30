using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Frontend.Client.Commands;

public record CreateRoleCommand(
    string Name,
    string? Description
) : ICommand<Guid>;
