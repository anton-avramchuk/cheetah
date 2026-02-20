using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record CreateRoleCommand(string Name) : ICommand<Guid>;
