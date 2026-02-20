using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record DeleteUserCommand(Guid Id) : ICommand;
