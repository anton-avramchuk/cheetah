using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record UpdateRoleCommand(Guid Id, string Name) : ICommand;
