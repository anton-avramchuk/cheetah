using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Application.Commands;

/// <summary>
/// Command to confirm user email address
/// </summary>
public record ConfirmEmailCommand(
    Guid UserId
) : ICommand;
