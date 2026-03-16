using Cheetah.Core.CQRS;

namespace Cheetah.Modules.Identity.Application.Commands;

public record DeleteUserCommand(Guid Id) : ICommand;
