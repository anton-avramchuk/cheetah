using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record DeleteUserIdentityCommand(Guid Id) : ICommand;