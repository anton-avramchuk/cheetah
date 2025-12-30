using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Application.Commands;

/// <summary>
/// Command to create a new role
/// </summary>
public record CreateRoleCommand(
    string Name,
    string? Description = null
) : ICommand<Guid>;
