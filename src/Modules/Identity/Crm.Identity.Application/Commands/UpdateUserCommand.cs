using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record UpdateUserCommand(Guid Id, string UserName, string Email) : ICommand;
