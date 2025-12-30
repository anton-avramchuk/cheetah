using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Application.Commands;

/// <summary>
/// Command to change user password
/// </summary>
public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : ICommand;
