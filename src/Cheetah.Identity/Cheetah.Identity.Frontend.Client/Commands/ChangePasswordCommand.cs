using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Frontend.Client.Commands;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : ICommand;
