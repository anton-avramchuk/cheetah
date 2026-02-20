using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record DeleteRoleCommand(Guid Id) : ICommand;
